using SVSim.Database.Models;
using SVSim.Database.Repositories.Mission;

namespace SVSim.EmulatedEntrypoint.Services;

public sealed class ViewerMissionStateService : IViewerMissionStateService
{
    private const int DailySlot = 0;
    private const int WeeklySlotStart = 1;
    private const int WeeklySlotCount = 3;
    private const int LotTypeDaily = 6;
    private const int LotTypeWeekly = 2;

    private readonly IMissionCatalogRepository _catalog;
    private readonly IViewerMissionRepository _viewerRepo;
    private readonly TimeProvider _time;

    public ViewerMissionStateService(
        IMissionCatalogRepository catalog,
        IViewerMissionRepository viewerRepo,
        TimeProvider time)
    {
        _catalog = catalog;
        _viewerRepo = viewerRepo;
        _time = time;
    }

    public async Task EnsureCurrentAsync(long viewerId, CancellationToken ct = default)
    {
        var existingAchievements = await _viewerRepo.GetAchievementsAsync(viewerId, ct);
        var existingMissions = await _viewerRepo.GetMissionsAsync(viewerId, ct);

        await MaterializeAchievementsAsync(viewerId, existingAchievements, ct);
        await EnsureMissionSlotsAsync(viewerId, existingMissions, ct);
    }

    private async Task MaterializeAchievementsAsync(long viewerId, List<ViewerAchievement> existing, CancellationToken ct)
    {
        var minLevelByType = await _catalog.GetMinLevelByAchievementTypeAsync(ct);
        if (minLevelByType.Count == 0) return;
        var existingTypes = existing.Select(a => a.AchievementType).ToHashSet();
        foreach (var (type, minLevel) in minLevelByType)
        {
            if (existingTypes.Contains(type)) continue;
            // Start at the lowest captured tier — with captured-data-is-catalog, a "fresh" viewer
            // is conceptually at whichever tier we first know about for that type.
            _viewerRepo.AddAchievement(new ViewerAchievement
            {
                ViewerId = viewerId,
                AchievementType = type,
                Level = minLevel,
                AchievementStatus = 0,
                NowAchievedLevel = 0,
                ResultAnnounceSawLevel = 0,
            });
        }
    }

    private async Task EnsureMissionSlotsAsync(long viewerId, List<ViewerMission> existing, CancellationToken ct)
    {
        var bySlot = existing.ToDictionary(m => m.Slot);
        var now = _time.GetUtcNow().ToUnixTimeSeconds();

        // Daily slot (slot 0)
        if (!bySlot.ContainsKey(DailySlot))
        {
            var pool = await _catalog.GetByLotTypeAsync(LotTypeDaily, ct);
            if (pool.Count > 0)
            {
                var pick = pool[Random.Shared.Next(pool.Count)];
                _viewerRepo.AddMission(new ViewerMission
                {
                    ViewerId = viewerId,
                    MissionCatalogId = pick.Id,
                    Slot = DailySlot,
                    AssignedAt = now,
                    MissionStatus = 1,
                });
            }
        }

        // Weekly slots (1..3) — assign all-or-nothing for simplicity in v1.
        bool weeklyNeedsAssignment = Enumerable.Range(WeeklySlotStart, WeeklySlotCount)
            .Any(s => !bySlot.ContainsKey(s));
        if (weeklyNeedsAssignment)
        {
            var pool = await _catalog.GetByLotTypeAsync(LotTypeWeekly, ct);
            if (pool.Count >= WeeklySlotCount)
            {
                var alreadyAssigned = existing
                    .Where(m => m.Slot >= WeeklySlotStart && m.Slot < WeeklySlotStart + WeeklySlotCount)
                    .Select(m => m.MissionCatalogId).ToHashSet();
                var available = pool.Where(p => !alreadyAssigned.Contains(p.Id)).ToList();
                var shuffled = available.OrderBy(_ => Random.Shared.Next()).ToList();

                int pickIdx = 0;
                for (int slot = WeeklySlotStart; slot < WeeklySlotStart + WeeklySlotCount; slot++)
                {
                    if (bySlot.ContainsKey(slot)) continue;
                    if (pickIdx >= shuffled.Count) break;
                    _viewerRepo.AddMission(new ViewerMission
                    {
                        ViewerId = viewerId,
                        MissionCatalogId = shuffled[pickIdx++].Id,
                        Slot = slot,
                        AssignedAt = now,
                        MissionStatus = 1,
                    });
                }
            }
        }
    }
}
