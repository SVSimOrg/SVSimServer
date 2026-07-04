using Microsoft.AspNetCore.Mvc;
using SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /ranking/* — Rankings menu. Stub: the period picker renders a real
/// deterministic monthly schedule, but every leaderboard returns an empty
/// `ranking: []`. See docs/superpowers/specs/2026-06-10-ranking-stubs-design.md.
/// </summary>
[Route("ranking")]
public sealed class RankingController : SVSimController
{
    [HttpPost("get_viewable_ranking_period_list")]
    public IActionResult GetViewablePeriodList([FromBody] BaseRequest req)
    {
        if (!TryGetViewerId(out _)) return Unauthorized();
        var now = DateTime.UtcNow;
        return Ok(new PeriodListResponseDto
        {
            RankMatch    = ToBase(RankingPeriodSchedule.GenerateFor(RankingPeriodSchedule.Family.RankMatch, now)),
            MasterPoint  = ToMasterPoint(RankingPeriodSchedule.GenerateFor(RankingPeriodSchedule.Family.MasterPoint, now)),
            TwoPick      = ToTwoPick(RankingPeriodSchedule.GenerateFor(RankingPeriodSchedule.Family.TwoPick, now)),
            Sealed       = ToBase(RankingPeriodSchedule.GenerateFor(RankingPeriodSchedule.Family.Sealed, now)),
            // Crossover arrays stay empty — captured prod returned [] for both.
        });
    }

    [HttpPost("master_point_rotation_info")]
    public IActionResult MasterPointRotation([FromBody] MasterPointInfoRequestDto req)
        => RankingFor(RankingPeriodSchedule.Family.MasterPoint, req.PeriodId);

    [HttpPost("master_point_unlimited_info")]
    public IActionResult MasterPointUnlimited([FromBody] MasterPointInfoRequestDto req)
        => RankingFor(RankingPeriodSchedule.Family.MasterPoint, req.PeriodId);

    [HttpPost("rank_match_class_win_rotation_info")]
    public IActionResult RankMatchClassWinRotation([FromBody] ClassWinInfoRequestDto req)
        => RankingFor(RankingPeriodSchedule.Family.RankMatch, req.PeriodId);

    [HttpPost("rank_match_class_win_unlimited_info")]
    public IActionResult RankMatchClassWinUnlimited([FromBody] ClassWinInfoRequestDto req)
        => RankingFor(RankingPeriodSchedule.Family.RankMatch, req.PeriodId);

    [HttpPost("two_pick_win_info")]
    public IActionResult TwoPickWin([FromBody] TwoPickWinInfoRequestDto req)
        => RankingFor(RankingPeriodSchedule.Family.TwoPick, req.PeriodId);

    private IActionResult RankingFor(RankingPeriodSchedule.Family family, int periodId)
    {
        if (!TryGetViewerId(out _)) return Unauthorized();
        var entry = RankingPeriodSchedule.TryFindById(family, periodId, DateTime.UtcNow);
        var periodDto = entry is null
            ? new PeriodEntryDto { Id = periodId.ToString() }
            : new PeriodEntryDto
            {
                Id = entry.Id,
                PeriodNum = entry.PeriodNum,
                BeginTime = entry.BeginTime,
                EndTime = entry.EndTime,
            };
        return Ok(new MonthlyRankingResponseDto { Period = periodDto, Ranking = new() });
    }

    private static List<PeriodEntryDto> ToBase(IReadOnlyList<PeriodEntry> src)
        => src.Select(e => new PeriodEntryDto
        {
            Id = e.Id, PeriodNum = e.PeriodNum, BeginTime = e.BeginTime, EndTime = e.EndTime,
        }).ToList();

    private static List<MasterPointPeriodEntryDto> ToMasterPoint(IReadOnlyList<PeriodEntry> src)
        => src.Select(e => new MasterPointPeriodEntryDto
        {
            Id = e.Id, PeriodNum = e.PeriodNum, BeginTime = e.BeginTime, EndTime = e.EndTime,
            NecessaryScore = "0",
        }).ToList();

    private static List<TwoPickPeriodEntryDto> ToTwoPick(IReadOnlyList<PeriodEntry> src)
        => src.Select(e => new TwoPickPeriodEntryDto
        {
            Id = e.Id, PeriodNum = e.PeriodNum, BeginTime = e.BeginTime, EndTime = e.EndTime,
            Type = "2", Over460 = "1",
        }).ToList();
}
