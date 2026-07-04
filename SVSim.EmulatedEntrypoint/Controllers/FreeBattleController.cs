using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.BattleNode.Bridge;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;
using SVSim.Database.Services.Friend;
using SVSim.Database.Services.Replay;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Matching;
using SVSim.EmulatedEntrypoint.Models.Dtos.FreeBattle;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Free battle family — casual / unranked human PvP. No AI variant exists in the client
/// (see ApiType.cs), so the controller has no /start, no AI /finish branch, and no
/// rank-progression fields on the response. Multi-prefix URLs (rotation_free_battle/,
/// unlimited_free_battle/, free_battle/force_finish) use explicit absolute route
/// attributes per action — does not extend SVSimController's [Route("[controller]")].
///
/// Mirrors RankBattleController for now; consolidation deferred — see the design doc.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = SteamAuthenticationConstants.SchemeName)]
public sealed class FreeBattleController : ControllerBase
{
    private readonly IMatchingResolver _resolver;
    private readonly IMatchContextBuilder _ctxBuilder;
    private readonly IBattleContextStore _battleContextStore;
    private readonly IBattleHistoryWriter _historyWriter;
    private readonly IPlayedTogetherWriter _playedTogetherWriter;
    private readonly IViewerRepository _viewers;
    private readonly IBattleXpService _xp;
    private readonly IMissionProgressService _missionProgress;
    private readonly SVSimDbContext _db;
    private readonly ILogger<FreeBattleController> _log;

    public FreeBattleController(
        IMatchingResolver resolver,
        IMatchContextBuilder ctxBuilder,
        IBattleContextStore battleContextStore,
        IBattleHistoryWriter historyWriter,
        IPlayedTogetherWriter playedTogetherWriter,
        IViewerRepository viewers,
        IBattleXpService xp,
        IMissionProgressService missionProgress,
        SVSimDbContext db,
        ILogger<FreeBattleController> log)
    {
        _resolver = resolver;
        _ctxBuilder = ctxBuilder;
        _battleContextStore = battleContextStore;
        _historyWriter = historyWriter;
        _playedTogetherWriter = playedTogetherWriter;
        _viewers = viewers;
        _xp = xp;
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

    [HttpPost("/rotation_free_battle/do_matching")]
    public Task<IActionResult> DoMatchingRotation([FromBody] DoMatchingRequestDto req, CancellationToken ct)
        => DoMatchingInternal("rotation_free_battle", Format.Rotation, req, ct);

    [HttpPost("/unlimited_free_battle/do_matching")]
    public Task<IActionResult> DoMatchingUnlimited([FromBody] DoMatchingRequestDto req, CancellationToken ct)
        => DoMatchingInternal("unlimited_free_battle", Format.Unlimited, req, ct);

    /// <summary>
    /// Shared finish handler — FreeBattleFinishTask sends the same wire shape across
    /// all six format URLs (only rotation + unlimited are routed here in v1).
    /// </summary>
    [HttpPost("/rotation_free_battle/finish")]
    [HttpPost("/unlimited_free_battle/finish")]
    public async Task<IActionResult> Finish([FromBody] FreeBattleFinishRequestDto req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var ctx = _battleContextStore.TakeFor(vid);
        bool isWin = req.BattleResult == 1;

        await _historyWriter.RecordAsync(vid, ctx, isWin, ct);

        // Played-together only fires for human PvP. Free battle has no AI variant, so
        // any non-null ctx with an OpponentViewerId > 0 is a real PvP pair.
        if (ctx is { OpponentViewerId: > 0 })
        {
            await _playedTogetherWriter.RecordAsync(
                vid,
                ctx.OpponentViewerId,
                new BattleParticipationContext(
                    PlayedMode: 0,
                    // Wire battle_type: 3 = free battle (per docs/api-spec/common/types.ts.md
                    // — NetworkDefine.ServerBattleType.Free).
                    BattleType: 3,
                    DeckFormat: ctx.DeckFormat,
                    TwoPickType: ctx.TwoPickType),
                ct);
        }

        int gainXp = 0, totalXp = 0, level = 1;
        bool leveledUp = false;
        var viewer = await _viewers.LoadForBattleXpGrantAsync(vid, ct);
        if (viewer is not null)
        {
            var xp = await _xp.GrantAsync(viewer, req.ClassId, isWin, BattleXpMode.Free, ct);
            await _db.SaveChangesAsync(ct);
            gainXp = xp.GetXp;
            totalXp = xp.TotalXp;
            level = xp.Level == 0 ? 1 : xp.Level;
            leveledUp = xp.LeveledUp;
        }

        // Mission counters — unranked wins feed ranked_or_arena_win + daily_match_win.
        if (isWin)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.Free.WinAll(), ct: ct);
        }
        if (leveledUp)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.ClassLevel.UpAll(req.ClassId), ct: ct);
        }

        return Ok(new FreeBattleFinishResponseDto
        {
            BattleResult = req.BattleResult,
            GetClassExperience = gainXp,
            ClassExperience = totalXp,
            ClassLevel = level,
        });
    }

    /// <summary>
    /// Defensive no-op for the family-wide force_finish URL. Per
    /// Shadowverse_Code_2026-05-23/ApiType.cs the URL is registry-only — no client task
    /// constructs it. Returning empty {} matches the rank-battle treatment.
    /// </summary>
    [HttpPost("/free_battle/force_finish")]
    public IActionResult ForceFinish([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var _u)) return Unauthorized();
        return Ok(new { });
    }

    private async Task<IActionResult> DoMatchingInternal(
        string mode, Format format, DoMatchingRequestDto req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        MatchContext ctx;
        try
        {
            // BuildForRankBattleAsync is name-misleading — it builds a generic MatchContext
            // from (viewer, format, deckNo). Rename slated for cleanup at consolidation time.
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

        return Ok(new DoMatchingResponseDto
        {
            MatchingState = r.MatchingState,
            BattleId = r.BattleId,
            NodeServerUrl = r.NodeServerUrl,
            // Placeholder per spec — per-battle card-master split is deferred.
            CardMasterId = 0,
        });
    }
}
