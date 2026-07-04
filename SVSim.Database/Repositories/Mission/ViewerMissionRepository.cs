using Microsoft.EntityFrameworkCore;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Mission;

public sealed class ViewerMissionRepository : IViewerMissionRepository
{
    private readonly SVSimDbContext _db;

    public ViewerMissionRepository(SVSimDbContext db) { _db = db; }

    public Task<List<ViewerMission>> GetMissionsAsync(long viewerId, CancellationToken ct) =>
        _db.ViewerMissions.Where(e => e.ViewerId == viewerId).OrderBy(e => e.Slot).ToListAsync(ct);

    public Task<ViewerMission?> GetMissionByIdAsync(long viewerId, long missionId, CancellationToken ct) =>
        _db.ViewerMissions.FirstOrDefaultAsync(e => e.ViewerId == viewerId && e.Id == missionId, ct);

    public Task<List<ViewerAchievement>> GetAchievementsAsync(long viewerId, CancellationToken ct) =>
        _db.ViewerAchievements.Where(e => e.ViewerId == viewerId).ToListAsync(ct);

    public Task<ViewerAchievement?> GetAchievementAsync(long viewerId, int achievementType, CancellationToken ct) =>
        _db.ViewerAchievements.FirstOrDefaultAsync(
            e => e.ViewerId == viewerId && e.AchievementType == achievementType, ct);

    public Task<List<ViewerEventCounter>> GetCountersAsync(
        long viewerId,
        IReadOnlyCollection<string> eventKeys,
        IReadOnlyCollection<string> periods,
        CancellationToken ct)
    {
        if (eventKeys.Count == 0 || periods.Count == 0) return Task.FromResult(new List<ViewerEventCounter>());
        return _db.ViewerEventCounters.AsNoTracking()
            .Where(e => e.ViewerId == viewerId
                        && eventKeys.Contains(e.EventKey)
                        && periods.Contains(e.Period))
            .ToListAsync(ct);
    }

    public async Task<int> GetCounterAsync(long viewerId, string eventKey, string period, CancellationToken ct)
    {
        var row = await _db.ViewerEventCounters.AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.ViewerId == viewerId && e.EventKey == eventKey && e.Period == period, ct);
        return row?.Count ?? 0;
    }

    public void AddMission(ViewerMission row) => _db.ViewerMissions.Add(row);
    public void RemoveMission(ViewerMission row) => _db.ViewerMissions.Remove(row);
    public void AddAchievement(ViewerAchievement row) => _db.ViewerAchievements.Add(row);

    public async Task UpsertCounterAsync(long viewerId, string eventKey, string period, int delta, CancellationToken ct)
    {
        var row = await _db.ViewerEventCounters.FirstOrDefaultAsync(
            e => e.ViewerId == viewerId && e.EventKey == eventKey && e.Period == period, ct);
        if (row is null)
        {
            _db.ViewerEventCounters.Add(new ViewerEventCounter
            {
                ViewerId = viewerId, EventKey = eventKey, Period = period, Count = delta,
            });
        }
        else
        {
            row.Count += delta;
        }
    }
}
