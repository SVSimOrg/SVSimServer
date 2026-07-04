using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Repositories.Globals;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Arena;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Arena;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Generic /arena/* family — primarily challenge-history info read by the TK2 entry screen's
/// detail button. TODO: lifetime TK2 stats tracking; today we emit a stub.
/// </summary>
[Route("arena")]
public class ArenaController : SVSimController
{
    private readonly IGlobalsRepository _globalsRepository;

    public ArenaController(IGlobalsRepository globalsRepository)
    {
        _globalsRepository = globalsRepository;
    }

    [HttpPost("get_challenge_info")]
    public async Task<IActionResult> GetChallengeInfo([FromBody] GetChallengeInfoRequest req)
    {
        if (!TryGetViewerId(out _)) return Unauthorized();

        var season = await _globalsRepository.GetCurrentArenaSeason();
        // Best-effort: pull begin/end_time + name from the season seed when present; otherwise
        // emit deterministic stub values. All 6 ChallangeHistoryInfoTask.Parse fields must be
        // present — the parser accesses them unconditionally.
        var beginTime = "2026-05-01 02:00:00";
        var endTime = "2026-06-01 01:59:59";
        var name = "Take Two";
        if (season is not null && !string.IsNullOrEmpty(season.FormatInfo) && season.FormatInfo != "{}")
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(season.FormatInfo);
                if (doc.RootElement.TryGetProperty("start_time", out var st)) beginTime = st.GetString() ?? beginTime;
                if (doc.RootElement.TryGetProperty("end_time", out var et))   endTime = et.GetString() ?? endTime;
                if (doc.RootElement.TryGetProperty("card_pool_name", out var cp)) name = cp.GetString() ?? name;
            }
            catch { /* fall back to defaults */ }
        }

        // Default Challenge Master reward steps from prod capture: 3 milestones at 5/10/15 wins.
        var rewardSteps = new Dictionary<string, string>
        {
            ["5"]  = "5",
            ["10"] = "10",
            ["15"] = "15",
        };

        return Ok(new GetChallengeInfoResponseDto
        {
            ChallengeName = name,
            BeginTime = beginTime,
            EndTime = endTime,
            TwoPickAllWinCount = 0,
            RewardStepInfo = new RewardStepInfoDto
            {
                MaxRewardStep = 15,
                RewardStepList = rewardSteps,
            },
        });
    }

    [HttpPost("get_challenge_ranking_history")]
    public IActionResult GetChallengeRankingHistory([FromBody] GetChallengeInfoRequest req)
    {
        if (!TryGetViewerId(out _)) return Unauthorized();
        // Prod returns {two_pick: [], sealed: []}. Stub matches.
        return Ok(new GetChallengeRankingHistoryResponseDto());
    }
}
