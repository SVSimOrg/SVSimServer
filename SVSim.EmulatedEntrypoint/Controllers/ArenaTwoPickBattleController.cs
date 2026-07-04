using Microsoft.AspNetCore.Mvc;
using SVSim.BattleNode.Bridge;
using SVSim.Database.Enums;
using SVSim.Database.Services;
using SVSim.Database.Services.Friend;
using SVSim.Database.Services.Replay;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Matching;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

[Route("arena_two_pick_battle")]
public class ArenaTwoPickBattleController : SVSimController
{
    private readonly IArenaTwoPickService _svc;
    private readonly IMatchContextBuilder _matchContextBuilder;
    private readonly IMatchingResolver _resolver;
    private readonly IBattleContextStore _battleContextStore;
    private readonly IBattleHistoryWriter _historyWriter;
    private readonly IPlayedTogetherWriter _playedTogetherWriter;
    private readonly IMissionProgressService _missionProgress;

    public ArenaTwoPickBattleController(
        IArenaTwoPickService svc,
        IMatchContextBuilder matchContextBuilder,
        IMatchingResolver resolver,
        IBattleContextStore battleContextStore,
        IBattleHistoryWriter historyWriter,
        IPlayedTogetherWriter playedTogetherWriter,
        IMissionProgressService missionProgress)
    {
        _svc = svc;
        _matchContextBuilder = matchContextBuilder;
        _resolver = resolver;
        _battleContextStore = battleContextStore;
        _historyWriter = historyWriter;
        _playedTogetherWriter = playedTogetherWriter;
        _missionProgress = missionProgress;
    }

    [HttpPost("do_matching")]
    public async Task<IActionResult> DoMatching(
        [FromBody] DoMatchingRequest req,
        CancellationToken ct = default)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        try
        {
            var ctx = await _matchContextBuilder.BuildForTwoPickAsync(vid);
            var r = await _resolver.ResolveAsync("arena_two_pick_battle", new BattlePlayer(vid, ctx), ct);

            if (r.BattleId is not null && long.TryParse(r.BattleId, out var battleIdLong))
            {
                _battleContextStore.Set(vid, new BattleContext(
                    BattleId:           battleIdLong,
                    // Two-pick wire battle_type — see docs/api-spec/common/types.ts.md
                    // #battle-types. Captured prod frames use 4 for both private match
                    // AND arena two-pick contexts; if a future capture disagrees, refine.
                    BattleType:         4,
                    DeckFormat:         Format.TwoPick.ToApi(),  // wire-int 10
                    TwoPickType:        0,                       // captured "0"; refine once tracked on MatchContext
                    SelfClassId:        (int)ctx.ClassId,        // CardClass enum
                    SelfSubClassId:     0,
                    SelfCharaId:        int.TryParse(ctx.CharaId, out var ch) ? ch : 0,
                    SelfRotationId:     "0",
                    // MatchContext (SVSim.BattleNode/Bridge/MatchContext.cs) does NOT carry
                    // opponent identity — the resolver returns only the BattleId. Leave
                    // opponent placeholders; when the two-pick matchmaking flow plumbs the
                    // second player's MatchContext through to the resolver result, fill
                    // these from there (and stash for both players).
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
            });
        }
        catch (ArenaTwoPickException ex)
        {
            return BadRequest(new { error_code = ex.ErrorCode });
        }
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish([FromBody] BattleFinishRequest req, CancellationToken ct = default)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        try
        {
            var battleCtx = _battleContextStore.TakeFor(vid);
            bool isWin = req.BattleResult == 1;

            await _historyWriter.RecordAsync(vid, battleCtx, isWin, ct);

            if (battleCtx is { OpponentViewerId: > 0 })
            {
                await _playedTogetherWriter.RecordAsync(
                    vid,
                    battleCtx.OpponentViewerId,
                    new BattleParticipationContext(
                        PlayedMode: 0,
                        BattleType: battleCtx.BattleType,
                        DeckFormat: battleCtx.DeckFormat,
                        TwoPickType: battleCtx.TwoPickType),
                    ct);
            }

            var result = await _svc.RecordBattleResultAsync(vid, isWin);

            // Mission counters — TK2 matches always advance challenge_play, wins additionally
            // advance challenge_win + the ranked/arena/daily aggregates.
            await _missionProgress.RecordEventAsync(
                vid,
                isWin ? MissionEventKeys.Challenge.MatchWinAll() : MissionEventKeys.Challenge.MatchPlayAll(),
                ct: ct);
            if (result.LeveledUp)
            {
                await _missionProgress.RecordEventAsync(
                    vid, MissionEventKeys.ClassLevel.UpAll(result.ClassId), ct: ct);
            }

            return Ok(new BattleFinishResponseDto
            {
                BattleResult = result.BattleResult,
                GetClassExperience = result.GetClassExperience,
                ClassExperience = result.ClassExperience,
                ClassLevel = result.ClassLevel,
                SpotPointInfo = new SpotPointInfoDto
                {
                    BeforeSpotPoint = result.BeforeSpotPoint,
                    AddSpotPoint = result.AddSpotPoint,
                    AfterSpotPoint = result.AfterSpotPoint,
                },
            });
        }
        catch (ArenaTwoPickException ex)
        {
            return BadRequest(new { error_code = ex.ErrorCode });
        }
    }
}
