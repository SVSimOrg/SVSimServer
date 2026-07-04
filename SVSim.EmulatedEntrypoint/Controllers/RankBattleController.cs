using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;
using SVSim.Database.Services.Friend;
using SVSim.Database.Services.RankProgress;
using SVSim.Database.Services.Replay;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Matching;
using SVSim.EmulatedEntrypoint.Models.Dtos.RankBattle;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Rank battle family — covers rotation/unlimited human PvP + AI variants. Crossover
/// is out of scope (no AI variant; human-only). Multi-prefix URLs (rotation_rank_battle/,
/// unlimited_rank_battle/, ai_*_rank_battle/, rank_battle/) require explicit absolute
/// route attributes on each action; the controller doesn't extend SVSimController's
/// [Route("[controller]")] convention.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = SteamAuthenticationConstants.SchemeName)]
public sealed class RankBattleController : ControllerBase
{
    private readonly IMatchingResolver _resolver;
    private readonly IBattleSessionStore _sessionStore;
    private readonly IMatchContextBuilder _ctxBuilder;
    private readonly IBotRoster _botRoster;
    private readonly IBattleContextStore _battleContextStore;
    private readonly IBattleHistoryWriter _historyWriter;
    private readonly IPlayedTogetherWriter _playedTogetherWriter;
    private readonly IViewerRepository _viewers;
    private readonly IBattleXpService _xp;
    private readonly IRankProgressService _rankProgress;
    private readonly IMissionProgressService _missionProgress;
    private readonly SVSimDbContext _db;
    private readonly ILogger<RankBattleController> _log;

    public RankBattleController(
        IMatchingResolver resolver,
        IBattleSessionStore sessionStore,
        IMatchContextBuilder ctxBuilder,
        IBotRoster botRoster,
        IBattleContextStore battleContextStore,
        IBattleHistoryWriter historyWriter,
        IPlayedTogetherWriter playedTogetherWriter,
        IViewerRepository viewers,
        IBattleXpService xp,
        IRankProgressService rankProgress,
        IMissionProgressService missionProgress,
        SVSimDbContext db,
        ILogger<RankBattleController> log)
    {
        _resolver = resolver;
        _sessionStore = sessionStore;
        _ctxBuilder = ctxBuilder;
        _botRoster = botRoster;
        _battleContextStore = battleContextStore;
        _historyWriter = historyWriter;
        _playedTogetherWriter = playedTogetherWriter;
        _viewers = viewers;
        _xp = xp;
        _rankProgress = rankProgress;
        _missionProgress = missionProgress;
        _db = db;
        _log = log;
    }

    private bool TryGetViewerId(out long viewerId)
    {
        viewerId = 0;
        var claim = User.Claims.FirstOrDefault(c => c.Type == ShadowverseClaimTypes.ViewerIdClaim)?.Value;
        return claim is not null && long.TryParse(claim, out viewerId);
    }

    [HttpPost("/rotation_rank_battle/do_matching")]
    public Task<IActionResult> DoMatchingRotation([FromBody] DoMatchingRequestDto req, CancellationToken ct)
        => DoMatchingInternal("rotation_rank_battle", Format.Rotation, req, ct);

    [HttpPost("/unlimited_rank_battle/do_matching")]
    public Task<IActionResult> DoMatchingUnlimited([FromBody] DoMatchingRequestDto req, CancellationToken ct)
        => DoMatchingInternal("unlimited_rank_battle", Format.Unlimited, req, ct);

    // AIBattleStartTask has no SetParameter override, so the body is just the inherited
    // PostParams (viewer_id / steam_id / steam_session_ticket) — but the translation
    // middleware requires at least one parameter to bind the decrypted body. Use BaseRequest.
    [HttpPost("/ai_rotation_rank_battle/start")]
    public Task<IActionResult> AiStartRotation([FromBody] BaseRequest _, CancellationToken ct)
        => AiStartInternal(Format.Rotation, ct);

    [HttpPost("/ai_unlimited_rank_battle/start")]
    public Task<IActionResult> AiStartUnlimited([FromBody] BaseRequest _, CancellationToken ct)
        => AiStartInternal(Format.Unlimited, ct);

    // Finish routes route by URL to the shared handler with the format baked in — the URL
    // is authoritative (mirrors DoMatchingRotation/DoMatchingUnlimited). Deriving format
    // from the BattleContext is fragile: the client may reach /finish without a prior
    // /do_matching (e.g. reconnect, replay-of-log flows), and we still need rank
    // progression to land in the right format bucket. Client-side, RankBattleFinishTask
    // picks the URL by (Data.CurrentFormat, IsAINetwork) — see decompile lines 12-35.

    [HttpPost("/rotation_rank_battle/finish")]
    public Task<IActionResult> FinishRotation([FromBody] RankBattleFinishRequestDto req, CancellationToken ct)
        => FinishInternal(Format.Rotation, req, ct);

    [HttpPost("/unlimited_rank_battle/finish")]
    public Task<IActionResult> FinishUnlimited([FromBody] RankBattleFinishRequestDto req, CancellationToken ct)
        => FinishInternal(Format.Unlimited, req, ct);

    [HttpPost("/ai_rotation_rank_battle/finish")]
    public Task<IActionResult> AiFinishRotation([FromBody] RankBattleFinishRequestDto req, CancellationToken ct)
        => FinishInternal(Format.Rotation, req, ct);

    [HttpPost("/ai_unlimited_rank_battle/finish")]
    public Task<IActionResult> AiFinishUnlimited([FromBody] RankBattleFinishRequestDto req, CancellationToken ct)
        => FinishInternal(Format.Unlimited, req, ct);

    /// <summary>
    /// Shared finish handler — RankBattleFinishTask parses the same wire shape for
    /// all four URLs. Grants class XP + rank progression (+100 win / -50 loss,
    /// tier-floored). Format is baked into the route.
    /// </summary>
    private async Task<IActionResult> FinishInternal(Format format, RankBattleFinishRequestDto req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var ctx = _battleContextStore.TakeFor(vid);
        bool isWin = req.BattleResult == 1;

        await _historyWriter.RecordAsync(vid, ctx, isWin, ct);

        // Played-together only fires for human PvP. AI bots have OpponentViewerId=0.
        if (ctx is { OpponentViewerId: > 0 })
        {
            await _playedTogetherWriter.RecordAsync(
                vid,
                ctx.OpponentViewerId,
                new BattleParticipationContext(
                    PlayedMode: 0,
                    BattleType: ctx.BattleType,
                    DeckFormat: ctx.DeckFormat,
                    TwoPickType: ctx.TwoPickType),
                ct);
        }

        int gainXp = 0, totalXp = 0, level = 1;
        bool leveledUp = false;
        var rankResult = new RankProgressResult(0, 0, 0, 0, 0, false, false);
        var viewer = await _viewers.LoadForRankProgressAsync(vid, ct);
        if (viewer is not null)
        {
            var xp = await _xp.GrantAsync(viewer, req.ClassId, isWin, BattleXpMode.Rank, ct);
            rankResult = await _rankProgress.GrantAsync(viewer, format, isWin, ct);
            await _db.SaveChangesAsync(ct);
            gainXp = xp.GetXp;
            totalXp = xp.TotalXp;
            level = xp.Level == 0 ? 1 : xp.Level;
            leveledUp = xp.LeveledUp;
        }

        // Mission counters — AI-rank routes emit the same keys as human-rank on a private server.
        if (isWin)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.Ranked.WinAll(req.ClassId), ct: ct);
        }
        if (leveledUp)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.ClassLevel.UpAll(req.ClassId), ct: ct);
        }
        if (rankResult.TierAdvanced)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.Rank.AchievedAll(rankResult.Rank), ct: ct);
        }

        return Ok(new RankBattleFinishResponseDto
        {
            BattleResult       = req.BattleResult,
            GetClassExperience = gainXp,
            ClassExperience    = totalXp,
            ClassLevel         = level,
            Rank               = rankResult.Rank,
            AfterBattlePoint   = rankResult.AfterBattlePoint,
            AfterMasterPoint   = rankResult.AfterMasterPoint,
            BattlePoint        = rankResult.BattlePoint,
            MasterPoint        = rankResult.MasterPoint,
        });
    }

    // BaseRequest parameter on every body-less action so the translation middleware can
    // bind the decrypted msgpack body (it explicitly requires at least one parameter).
    [HttpPost("/rank_battle/force_finish")]
    public IActionResult ForceFinish([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var _u)) return Unauthorized();
        return Ok(new { });
    }

    [HttpPost("/rank_battle/add_client_log")]
    [HttpPost("/rank_battle/add_all_client_log")]
    [HttpPost("/rank_battle/add_last_turn_log")]
    public IActionResult AddClientLog([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var _u)) return Unauthorized();
        return Ok(new { });
    }

    [HttpPost("/rank_battle/get_latest_master_point")]
    public IActionResult GetLatestMasterPoint([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var _u)) return Unauthorized();
        return Ok(new { });
    }

    private async Task<IActionResult> DoMatchingInternal(string mode, Format format, DoMatchingRequestDto req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        MatchContext ctx;
        try
        {
            ctx = await _ctxBuilder.BuildForRankBattleAsync(vid, format, req.DeckNo);
        }
        catch (InvalidOperationException ex)
        {
            // Most likely cause: viewer has no deck at that slot for this format. Surface
            // as 3001 RC_BATTLE_MATCHING_ILLEGAL — the client shows the standard
            // matchmaking-error dialog rather than retrying forever.
            _log.LogWarning(ex, "BuildForRankBattleAsync failed for viewer {Vid} format {Fmt} deckNo {DeckNo}; returning 3001.", vid, format, req.DeckNo);
            return Ok(new DoMatchingResponseDto { MatchingState = 3001, NodeServerUrl = "" });
        }

        var r = await _resolver.ResolveAsync(mode, new BattlePlayer(vid, ctx), ct);

        // Human-PvP path: stash battle context so the /finish handler can compose a
        // ViewerBattleHistory row. Mirrors the ArenaTwoPick and AI-start hooks. Without
        // this, /unlimited_rank_battle/finish and /rotation_rank_battle/finish always find
        // a null context and BattleHistoryWriter no-ops (see live server_log 2026-07-02).
        // Opponent identity is not yet plumbed through the resolver result (same gap as
        // ArenaTwoPick — see its comment); zero placeholders until MatchContext carries the
        // second player forward.
        if (r.BattleId is not null && long.TryParse(r.BattleId, out var battleIdLong))
        {
            _battleContextStore.Set(vid, new BattleContext(
                BattleId:           battleIdLong,
                // Wire battle_type: 2 = rank battle (per docs/api-spec/common/types.ts.md
                // #battle-types), same value the AI variant uses (mode is disambiguated
                // by URL, not by battle_type).
                BattleType:         2,
                DeckFormat:         format.ToApi(),
                TwoPickType:        0,
                SelfClassId:        (int)ctx.ClassId,
                SelfSubClassId:     0,
                SelfCharaId:        int.TryParse(ctx.CharaId, out var ch) ? ch : 0,
                SelfRotationId:     "0",
                OpponentViewerId:   0,
                OpponentName:       "",
                OpponentClassId:    0,
                OpponentSubClassId: 0,
                OpponentCharaId:    0,
                OpponentCountryCode: "",
                OpponentEmblemId:   0,
                OpponentDegreeId:   0,
                OpponentRotationId: "0",
                BattleStartTime:    DateTime.UtcNow));
        }

        return Ok(new DoMatchingResponseDto
        {
            MatchingState = r.MatchingState,
            BattleId = r.BattleId,
            NodeServerUrl = r.NodeServerUrl,
            // Placeholder per spec § Out of scope — per-battle card-master split is deferred.
            CardMasterId = 0,
        });
    }

    private async Task<IActionResult> AiStartInternal(Format format, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        // The /ai_<fmt>/start request body is BaseRequest only — it carries no deck_no.
        // The deck the viewer queued with was captured in the PendingBattle's MatchContext
        // at /do_matching resolution time (when InProcessPairUp called bridge.RegisterBattle).
        // Reuse that context so SelfInfo's classId/charaId/sleeveId match what the user
        // actually picked. Rebuilding from deck #1 was the 2026-06-02 wire-bug — surfaced
        // as "queued Bloodcraft, saw Swordcraft leader."
        var pending = _sessionStore.TryFindPendingForViewer(vid);
        if (pending is null)
        {
            _log.LogWarning("AiStart for viewer {Vid} format {Fmt} has no pending battle; returning ai_id=-1.", vid, format);
            return Ok(new AiBattleStartResponseDto { AiId = -1 });
        }
        var selfCtx = pending.P1.Context;

        var bot = await _botRoster.PickAsync(selfCtx, pending.BattleId, ct);
        var seed = Random.Shared.Next();

        // Read the viewer's rank progression for this format so the pre-battle screen
        // shows the current tier/point rather than "Beginner 0 with 0 pts."
        var selfViewer = await _viewers.LoadForRankProgressAsync(vid, ct);
        var selfRank = selfViewer is null
            ? new RankProgressResult(1, 0, 0, 0, 0, false, false)
            : await _rankProgress.GetAsync(selfViewer, format, ct);

        // Stash battle context for the upcoming /finish so the replay-history hook can
        // compose a ViewerBattleHistory row. See docs/superpowers/specs/2026-06-10-replay-info-design.md.
        if (long.TryParse(pending.BattleId, out var battleIdLong))
        {
            _battleContextStore.Set(vid, new BattleContext(
                BattleId:           battleIdLong,
                // Wire battle_type: 2 = rank battle (per docs/api-spec/common/types.ts.md
                // #battle-types). AI variant shares the rank-battle wire id.
                BattleType:         2,
                DeckFormat:         format.ToApi(),                                // wire-int via existing converter
                TwoPickType:        0,
                SelfClassId:        (int)selfCtx.ClassId,                          // CardClass enum
                SelfSubClassId:     0,
                SelfCharaId:        int.TryParse(selfCtx.CharaId, out var ch) ? ch : 0,  // CharaId is string on MatchContext
                SelfRotationId:     "0",
                OpponentViewerId:   0,                                             // AI bot — not a real viewer
                OpponentName:       bot.UserName,
                OpponentClassId:    bot.ClassId,                                   // int on AIBotProfile
                OpponentSubClassId: 0,
                OpponentCharaId:    bot.CharaId,                                   // int on AIBotProfile
                OpponentCountryCode: bot.CountryCode,
                OpponentEmblemId:   bot.EmblemId,                                  // int → long widen
                OpponentDegreeId:   bot.DegreeId,                                  // int → long widen
                OpponentRotationId: "0",
                BattleStartTime:    DateTime.UtcNow));
        }

        // Per spec, ai-start.md TODO: turnState semantics unclear. Default 0 (player first).
        return Ok(new AiBattleStartResponseDto
        {
            AiId = bot.AiId,
            TurnState = 0,
            SelfInfo = new AiBattlePlayerInfo
            {
                CountryCode = selfCtx.CountryCode,
                UserName = selfCtx.UserName,
                SleeveId = int.TryParse(selfCtx.SleeveId, out var sId) ? sId : -1,
                EmblemId = int.TryParse(selfCtx.EmblemId, out var eId) ? eId : -1,
                DegreeId = int.TryParse(selfCtx.DegreeId, out var dId) ? dId : -1,
                FieldId = selfCtx.FieldId,
                IsOfficial = selfCtx.IsOfficial,
                OppoId = bot.AiId,
                Seed = seed,
                Rank = selfRank.Rank,
                BattlePoint = selfRank.AfterBattlePoint,
                ClassId = (int)selfCtx.ClassId,
                CharaId = int.TryParse(selfCtx.CharaId, out var chId) ? chId : -1,
                IsMasterRank = selfRank.IsMasterRank ? 1 : 0,
                MasterPoint = selfRank.AfterMasterPoint,
            },
            OppoInfo = new AiBattlePlayerInfo
            {
                CountryCode = bot.CountryCode,
                UserName = bot.UserName,
                SleeveId = bot.SleeveId,
                EmblemId = bot.EmblemId,
                DegreeId = bot.DegreeId,
                FieldId = bot.FieldId,
                IsOfficial = bot.IsOfficial,
                OppoId = (int)vid,
                Seed = seed,
                Rank = bot.Rank,
                BattlePoint = bot.BattlePoint,
                ClassId = bot.ClassId,
                CharaId = bot.CharaId,
                IsMasterRank = bot.IsMasterRank,
                MasterPoint = bot.MasterPoint,
            },
        });
    }
}
