using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Database.Repositories.BattlePass;

public sealed class ViewerBattlePassRepository : IViewerBattlePassRepository
{
    private readonly SVSimDbContext _db;

    public ViewerBattlePassRepository(SVSimDbContext db) { _db = db; }

    public async Task<ViewerBattlePassProgressEntry> GetOrCreateProgressAsync(long viewerId, int seasonId, CancellationToken ct)
    {
        var existing = await _db.ViewerBattlePassProgress
            .FirstOrDefaultAsync(p => p.ViewerId == viewerId && p.SeasonId == seasonId, ct);
        if (existing is not null) return existing;

        var entry = new ViewerBattlePassProgressEntry
        {
            ViewerId = viewerId,
            SeasonId = seasonId,
            CurrentPoint = 0,
            IsPremium = false,
            WeeklyPoints = 0,
            WeeklyPeriodStart = null,
        };
        _db.ViewerBattlePassProgress.Add(entry);
        try
        {
            await _db.SaveChangesAsync(ct);
            return entry;
        }
        catch (DbUpdateException)
        {
            // Concurrent /info call won the race; re-read the row the other thread persisted.
            _db.Entry(entry).State = EntityState.Detached;
            return await _db.ViewerBattlePassProgress
                .FirstAsync(p => p.ViewerId == viewerId && p.SeasonId == seasonId, ct);
        }
    }

    public Task<List<ViewerBattlePassClaimEntry>> GetClaimsAsync(long viewerId, int seasonId, CancellationToken ct) =>
        _db.ViewerBattlePassClaims.AsNoTracking()
            .Where(c => c.ViewerId == viewerId && c.SeasonId == seasonId)
            .ToListAsync(ct);

    public void AddClaim(long viewerId, int seasonId, BattlePassTrack track, int level, DateTimeOffset claimedAt)
    {
        _db.ViewerBattlePassClaims.Add(new ViewerBattlePassClaimEntry
        {
            ViewerId = viewerId, SeasonId = seasonId, Track = track,
            Level = level, ClaimedAt = claimedAt,
        });
    }
}
