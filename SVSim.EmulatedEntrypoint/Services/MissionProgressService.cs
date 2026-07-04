using SVSim.Database;
using SVSim.Database.Repositories.Mission;

namespace SVSim.EmulatedEntrypoint.Services;

public sealed class MissionProgressService : IMissionProgressService
{
    private readonly SVSimDbContext _db;
    private readonly IMissionCatalogRepository _catalog;
    private readonly IViewerMissionRepository _viewerRepo;
    private readonly TimeProvider _time;
    private readonly IGameCalendarService _calendar;

    public MissionProgressService(
        SVSimDbContext db,
        IMissionCatalogRepository catalog,
        IViewerMissionRepository viewerRepo,
        TimeProvider time,
        IGameCalendarService calendar)
    {
        _db = db;
        _catalog = catalog;
        _viewerRepo = viewerRepo;
        _time = time;
        _calendar = calendar;
    }

    public async Task RecordEventAsync(long viewerId, IReadOnlyList<string> eventKeys, int delta = 1, CancellationToken ct = default)
    {
        if (eventKeys.Count == 0) return;
        var now = _time.GetUtcNow();
        var periods = _calendar.AllPeriods(now);

        // 1. Increment counters for every (key, period).
        foreach (var key in eventKeys)
        {
            foreach (var period in periods)
            {
                await _viewerRepo.UpsertCounterAsync(viewerId, key, period, delta, ct);
            }
        }
        await _db.SaveChangesAsync(ct);

        // 2. Find catalog rows referencing any of these event keys; mark claimable on threshold.
        var achievements = await _catalog.GetAchievementsByEventTypesAsync(eventKeys, ct);
        if (achievements.Count > 0)
        {
            var byType = achievements.GroupBy(a => a.AchievementType).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var (achType, catalogRows) in byType)
            {
                var viewerRow = await _viewerRepo.GetAchievementAsync(viewerId, achType, ct);
                if (viewerRow is null) continue;
                var atLevel = catalogRows.FirstOrDefault(r => r.Level == viewerRow.Level);
                if (atLevel is null || atLevel.EventType is null) continue;

                var count = await _viewerRepo.GetCounterAsync(viewerId, atLevel.EventType, GameCalendarPeriods.AllTime, ct);
                if (count >= atLevel.RequireNumber && viewerRow.AchievementStatus == 0)
                {
                    viewerRow.AchievementStatus = 1;
                }
            }
            await _db.SaveChangesAsync(ct);
        }
    }
}
