using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Services.Replay;

public sealed class ReplayHistoryReader : IReplayHistoryReader
{
    private readonly SVSimDbContext _db;

    public ReplayHistoryReader(SVSimDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReplayHistoryEntry>> GetRecentAsync(long viewerId, int take, CancellationToken ct)
    {
        return await _db.ViewerBattleHistories
            .AsNoTracking()
            .Where(h => h.ViewerId == viewerId)
            .OrderByDescending(h => h.CreateTime)
            .Take(take)
            .Select(h => new ReplayHistoryEntry(
                h.BattleId, h.BattleType, h.DeckFormat, h.TwoPickType, h.IsLimitTurn,
                h.SelfClassId, h.SelfSubClassId, h.SelfCharaId, h.SelfRotationId,
                h.OpponentClassId, h.OpponentSubClassId, h.OpponentCharaId,
                h.OpponentName, h.OpponentCountryCode,
                h.OpponentEmblemId, h.OpponentDegreeId, h.OpponentRotationId,
                h.IsWin, h.BattleStartTime, h.CreateTime))
            .ToListAsync(ct);
    }
}
