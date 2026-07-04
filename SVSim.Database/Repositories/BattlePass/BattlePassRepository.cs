using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.BattlePass;

public sealed class BattlePassRepository : IBattlePassRepository
{
    private readonly SVSimDbContext _db;
    private readonly IMemoryCache _cache;

    // Per-host cache for the immutable level curve, scoped via the DI-registered IMemoryCache.
    // In production "host == process"; in tests each WebApplicationFactory builds its own
    // service provider so the cache is naturally isolated per fixture — avoids the pre-refactor
    // race where a process-static cache populated from one test's DbContext served stale data
    // to a parallel test reading from a different DB.
    private const string LevelCurveCacheKey = "battlepass:level-curve";

    public BattlePassRepository(SVSimDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<BattlePassSeasonEntry?> GetActiveSeasonAsync(DateTimeOffset when, CancellationToken ct)
    {
        // Use UtcDateTime for the LINQ comparison so the query translates on both Postgres and
        // SQLite. DateTimeOffset arithmetic in LINQ isn't supported by the SQLite provider;
        // DateTime (UTC) is stored and compared as ISO-8601 text which SQLite handles fine.
        var utcNow = when.UtcDateTime;
        var candidates = await _db.BattlePassSeasons
            .AsNoTracking()
            .ToListAsync(ct);
        return candidates
            .Where(s => s.StartDate.UtcDateTime <= utcNow && s.EndDate.UtcDateTime > utcNow)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();
    }

    public Task<BattlePassSeasonEntry?> GetSeasonAsync(int seasonId, CancellationToken ct) =>
        _db.BattlePassSeasons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == seasonId, ct);

    public async Task<List<BattlePassRewardEntry>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct) =>
        await _db.BattlePassRewards.AsNoTracking()
            .Where(r => r.SeasonId == seasonId)
            .OrderBy(r => r.Track).ThenBy(r => r.Level)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BattlePassLevelEntry>> GetLevelCurveAsync(CancellationToken ct)
    {
        var cached = await _cache.GetOrCreateAsync(LevelCurveCacheKey, async _ =>
            (IReadOnlyList<BattlePassLevelEntry>)await _db.BattlePassLevels.AsNoTracking()
                .OrderBy(e => e.Level)
                .ToListAsync(ct));
        return cached!;
    }
}
