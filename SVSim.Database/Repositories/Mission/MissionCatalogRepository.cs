using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Mission;

public sealed class MissionCatalogRepository : IMissionCatalogRepository
{
    private readonly SVSimDbContext _db;
    private readonly IMemoryCache _cache;

    // Per-host cache for the derived MAX(Level) lookup, scoped via the DI-registered
    // IMemoryCache. See BattlePassRepository for the per-host rationale (same parallel-test
    // race avoidance — each WebApplicationFactory gets its own cache).
    private const string MaxLevelCacheKey = "mission:achievement-max-level-by-type";

    public MissionCatalogRepository(SVSimDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public Task<List<MissionCatalogEntry>> GetByLotTypeAsync(int lotType, CancellationToken ct) =>
        _db.MissionCatalog.AsNoTracking().Where(e => e.LotType == lotType).ToListAsync(ct);

    public Task<List<MissionCatalogEntry>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        _db.MissionCatalog.AsNoTracking().Where(e => ids.Contains(e.Id)).ToListAsync(ct);

    public Task<MissionCatalogEntry?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.MissionCatalog.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<List<MissionCatalogEntry>> GetByEventTypesAsync(IReadOnlyCollection<string> eventTypes, CancellationToken ct) =>
        _db.MissionCatalog.AsNoTracking()
            .Where(e => e.EventType != null && eventTypes.Contains(e.EventType))
            .ToListAsync(ct);

    public Task<List<AchievementCatalogEntry>> GetAchievementsByEventTypesAsync(IReadOnlyCollection<string> eventTypes, CancellationToken ct) =>
        _db.AchievementCatalog.AsNoTracking()
            .Where(e => e.EventType != null && eventTypes.Contains(e.EventType))
            .ToListAsync(ct);

    public Task<List<int>> GetAllAchievementTypesAsync(CancellationToken ct) =>
        _db.AchievementCatalog.AsNoTracking()
            .Select(e => e.AchievementType).Distinct()
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<int, int>> GetMaxLevelByAchievementTypeAsync(CancellationToken ct)
    {
        var cached = await _cache.GetOrCreateAsync(MaxLevelCacheKey, async _ =>
        {
            var pairs = await _db.AchievementCatalog.AsNoTracking()
                .GroupBy(e => e.AchievementType)
                .Select(g => new { Type = g.Key, Max = g.Max(e => e.Level) })
                .ToListAsync(ct);
            return (IReadOnlyDictionary<int, int>)pairs.ToDictionary(p => p.Type, p => p.Max);
        });
        return cached!;
    }

    public async Task<IReadOnlyDictionary<int, int>> GetMinLevelByAchievementTypeAsync(CancellationToken ct)
    {
        var pairs = await _db.AchievementCatalog.AsNoTracking()
            .GroupBy(e => e.AchievementType)
            .Select(g => new { Type = g.Key, Min = g.Min(e => e.Level) })
            .ToListAsync(ct);
        return pairs.ToDictionary(p => p.Type, p => p.Min);
    }

    public Task<AchievementCatalogEntry?> GetAchievementAsync(int achievementType, int level, CancellationToken ct) =>
        _db.AchievementCatalog.AsNoTracking()
            .FirstOrDefaultAsync(e => e.AchievementType == achievementType && e.Level == level, ct);

    public Task<List<BattlePassMonthlyMissionEntry>> GetMonthlyMissionsAsync(int year, int month, CancellationToken ct) =>
        _db.BattlePassMonthlyMissions.AsNoTracking()
            .Where(e => e.Year == year && e.Month == month)
            .OrderBy(e => e.OrderNum).ToListAsync(ct);
}
