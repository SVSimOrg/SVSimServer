using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Repositories.Mission;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos.Achievement;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /achievement/* — claim achievement rewards. Wire shape mirrors AchievementReceiveRewardTask.cs.
/// </summary>
[Route("achievement")]
public class AchievementController : SVSimController
{
    private const int FailureResultCode = 2;

    private readonly SVSimDbContext _db;
    private readonly IMissionCatalogRepository _catalog;
    private readonly IViewerMissionStateService _state;
    private readonly IMissionAssembler _assembler;
    private readonly IInventoryService _inv;

    public AchievementController(
        SVSimDbContext db,
        IMissionCatalogRepository catalog,
        IViewerMissionStateService state,
        IMissionAssembler assembler,
        IInventoryService inv)
    {
        _db = db;
        _catalog = catalog;
        _state = state;
        _assembler = assembler;
        _inv = inv;
    }

    [HttpPost("receive_reward")]
    public async Task<IActionResult> ReceiveReward(
        AchievementReceiveRewardRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // EnsureCurrentAsync needs a viewer id — use a lightweight pre-check load then
        // materialize state before opening the inventory tx.
        var viewerIdCheck = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(ct);
        if (viewerIdCheck == 0) return Unauthorized();

        await _state.EnsureCurrentAsync(viewerId, ct);
        await _db.SaveChangesAsync(ct);

        // Re-read viewer's achievement for this type after state-service materialization.
        var ach = await _db.ViewerAchievements
            .FirstOrDefaultAsync(a => a.ViewerId == viewerId && a.AchievementType == request.AchievementType, ct);
        if (ach is null || ach.Level != request.Level)
        {
            return Ok(new { result_code = FailureResultCode });
        }

        var catalogRow = await _catalog.GetAchievementAsync(request.AchievementType, request.Level, ct);
        if (catalogRow is null)
        {
            return Ok(new { result_code = FailureResultCode });
        }

        // Open inventory tx and grant via InventoryService.
        await using var tx = await _inv.BeginAsync(viewerId, ct, cfg => cfg.Source = GrantSource.AchievementReward);

        var granted = await tx.GrantAsync(
            catalogRow.RewardType,
            catalogRow.RewardDetailId,
            catalogRow.RewardNumber,
            ct);

        // Advance viewer's level by 1. If no catalog row exists at the new level (i.e. just
        // claimed the highest captured tier), max_level on the wire stays the same and the
        // UI shows "claimed at max" until catalog grows.
        ach.Level += 1;
        var maxLevelByType = await _catalog.GetMaxLevelByAchievementTypeAsync(ct);
        if (maxLevelByType.TryGetValue(request.AchievementType, out int maxLevel)
            && ach.Level > maxLevel)
        {
            ach.AchievementStatus = 2;
        }
        else
        {
            ach.AchievementStatus = 0;
        }
        ach.NowAchievedLevel = request.Level;

        await tx.CommitAsync(ct);

        var dto = await _assembler.BuildAsync(tx.Viewer, ct);
        var resp = new AchievementReceiveRewardResponse
        {
            UserMissionList = dto.UserMissionList,
            UserAchievementList = dto.UserAchievementList,
            BattlePassMonthlyMission = dto.BattlePassMonthlyMission,
            IsChangeMission = dto.IsChangeMission,
            CanChangeMissionTime = dto.CanChangeMissionTime,
            IsChangeReceiveType = dto.IsChangeReceiveType,
            CanChangeReceiveTypeTime = dto.CanChangeReceiveTypeTime,
            MissionReceiveType = dto.MissionReceiveType,
            RewardList = granted.Select(g => new RewardGrantDto
            {
                RewardType = (int)g.RewardType,
                RewardId = g.RewardId,
                RewardNum = g.RewardNum,
            }).ToList(),
            TotalReceiveCountList = granted.Select(g => new TotalReceiveCountDto
            {
                RewardType = (int)g.RewardType,
                RewardDetailId = g.RewardId,
                RewardCount = g.RewardNum,
                ItemType = 0,
                IsUsable = true,
            }).ToList(),
        };

        return Ok(resp);
    }
}
