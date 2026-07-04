using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.BattleNode.Bridge;
using SVSim.Database;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Matching;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Security;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.EmulatedEntrypoint.Services.ArenaColosseum;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Per-match URLs for the Colosseum bracket — the dual <c>colosseum_battle/*</c> +
/// <c>colosseum_rank_battle/*</c> route family. Does NOT extend <see cref="SVSimController"/>'s
/// <c>[Route("[controller]")]</c> because we need explicit absolute routes for two prefixes
/// off one controller (same pattern as <see cref="FreeBattleController"/>).
/// <para>
/// The URL is the bracket-phase signal per do-matching.md §"server-side bidirectional
/// mapping"; <see cref="ViewerArenaColosseumRun.IsRankMatching"/> flips to true once the
/// node has signalled <c>matching_state == 3008</c> on a prior <c>do_matching</c>.
/// </para>
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = SteamAuthenticationConstants.SchemeName)]
public sealed class ArenaColosseumBattleController : ControllerBase
{
    /// <summary>Per FinishTaskBase.IsEffectiveErrorCode — code 3502 ("battle already finished")
    /// is tolerated as a non-error retry, parsed the same as 1.</summary>
    private const int BattleAlreadyFinishedResultCode = 3502;

    private readonly IMatchContextBuilder _ctxBuilder;
    private readonly IMatchingResolver _resolver;
    private readonly IArenaColosseumRunRepository _runs;
    private readonly IColosseumProgressionService _progression;
    private readonly IViewerRepository _viewers;
    private readonly IBattleXpService _xp;
    private readonly IMissionProgressService _missionProgress;
    private readonly SVSimDbContext _db;
    private readonly ILogger<ArenaColosseumBattleController> _log;

    public ArenaColosseumBattleController(
        IMatchContextBuilder ctxBuilder,
        IMatchingResolver resolver,
        IArenaColosseumRunRepository runs,
        IColosseumProgressionService progression,
        IViewerRepository viewers,
        IBattleXpService xp,
        IMissionProgressService missionProgress,
        SVSimDbContext db,
        ILogger<ArenaColosseumBattleController> log)
    {
        _ctxBuilder = ctxBuilder;
        _resolver = resolver;
        _runs = runs;
        _progression = progression;
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

    [HttpPost("/colosseum_battle/do_matching")]
    public Task<IActionResult> DoMatchingPreRank(
        [FromBody] ColosseumDoMatchingRequestDto req, CancellationToken ct)
        => DoMatchingInternal(isRankUrl: false, req, ct);

    [HttpPost("/colosseum_rank_battle/do_matching")]
    public Task<IActionResult> DoMatchingPostRank(
        [FromBody] ColosseumDoMatchingRequestDto req, CancellationToken ct)
        => DoMatchingInternal(isRankUrl: true, req, ct);

    [HttpPost("/colosseum_battle/finish")]
    public Task<IActionResult> FinishPreRank(
        [FromBody] ColosseumBattleFinishRequestDto req, CancellationToken ct)
        => FinishInternal(isRankUrl: false, req, ct);

    [HttpPost("/colosseum_rank_battle/finish")]
    public Task<IActionResult> FinishPostRank(
        [FromBody] ColosseumBattleFinishRequestDto req, CancellationToken ct)
        => FinishInternal(isRankUrl: true, req, ct);

    private async Task<IActionResult> DoMatchingInternal(
        bool isRankUrl, ColosseumDoMatchingRequestDto req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null)
        {
            return BadRequest(new { error = "arena_colosseum_no_active_run" });
        }
        if (isRankUrl != run.IsRankMatching)
        {
            return BadRequest(new
            {
                error = "colosseum_url_phase_mismatch",
                is_rank_matching = run.IsRankMatching,
                requested_rank_url = isRankUrl,
            });
        }

        MatchContext ctx;
        try
        {
            ctx = await _ctxBuilder.BuildForColosseumAsync(vid);
        }
        catch (InvalidOperationException ex)
        {
            _log.LogWarning(ex,
                "Colosseum BuildForColosseumAsync failed for viewer {Vid}; returning 3001.", vid);
            return Ok(new Models.Dtos.FreeBattle.DoMatchingResponseDto
            {
                MatchingState = 3001,
                NodeServerUrl = "",
            });
        }

        var mode = isRankUrl ? "colosseum_rank_battle" : "colosseum_battle";
        var resolution = await _resolver.ResolveAsync(mode, new BattlePlayer(vid, ctx), ct);

        // Promotion: server flips IsRankMatching once on the 3008 signal. Subsequent battle
        // URLs must use the rank prefix. (Plan §"matching_state == 3008 is the promotion trigger".)
        if (_progression.ShouldPromoteToRankMatching(run, resolution.MatchingState))
        {
            run.IsRankMatching = true;
            await _runs.UpsertAsync(run);
        }

        return Ok(new Models.Dtos.FreeBattle.DoMatchingResponseDto
        {
            MatchingState = resolution.MatchingState,
            BattleId = resolution.BattleId,
            NodeServerUrl = resolution.NodeServerUrl,
        });
    }

    private async Task<IActionResult> FinishInternal(
        bool isRankUrl, ColosseumBattleFinishRequestDto req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null)
        {
            return BadRequest(new { error = "arena_colosseum_no_active_run" });
        }

        // Match-result tracking: 1 = win, 2 = loss, 0 = draw/abort. Record_list mirrors
        // ColosseumTopTask.battle_results.result_list (1=win, 0=loss). is_retire surfaces
        // as a non-counted result — does not advance the bracket.
        bool counts = req.IsRetire == 0;
        if (counts)
        {
            bool isWin = req.BattleResult == 1;
            run.BattleCountThisRound += 1;
            if (isWin) run.WinCount += 1;
            else run.LossCount += 1;

            var resultList = ParseIntList(run.ResultListJson);
            resultList.Add(isWin ? 1 : 0);
            run.ResultListJson = JsonSerializer.Serialize(resultList);
            await _runs.UpsertAsync(run);
        }

        int gainXp = 0, totalXp = 0, level = 1;
        bool leveledUp = false;
        var viewer = await _viewers.LoadForBattleXpGrantAsync(vid, ct);
        if (viewer is not null)
        {
            var xp = await _xp.GrantAsync(viewer, req.ClassId, req.BattleResult == 1, BattleXpMode.Colosseum, ct);
            await _db.SaveChangesAsync(ct);
            gainXp = xp.GetXp;
            totalXp = xp.TotalXp;
            level = xp.Level == 0 ? 1 : xp.Level;
            leveledUp = xp.LeveledUp;
        }

        if (leveledUp)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.ClassLevel.UpAll(req.ClassId), ct: ct);
        }

        // result_code 3502 ("battle already finished") is the idempotent-retry tolerance per
        // FinishTaskBase.IsEffectiveErrorCode. Server emits standard data; the translation
        // middleware sets the wire result_code via data_headers from the response envelope.
        return Ok(new ColosseumBattleFinishResponseDto
        {
            BattleResult = req.BattleResult,
            GetClassExperience = gainXp,
            ClassExperience = totalXp,
            ClassLevel = level,
        });
    }

    private static List<int> ParseIntList(string json) =>
        string.IsNullOrEmpty(json)
            ? new()
            : JsonSerializer.Deserialize<List<int>>(json) ?? new();
}
