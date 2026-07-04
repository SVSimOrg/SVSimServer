using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SVSim.Database.Models;

namespace SVSim.Database.Services.Replay;

public sealed class BattleHistoryWriter : IBattleHistoryWriter
{
    internal const int RetentionCap = 50;

    private readonly SVSimDbContext _db;
    private readonly ILogger<BattleHistoryWriter> _log;

    public BattleHistoryWriter(SVSimDbContext db, ILogger<BattleHistoryWriter> log)
    {
        _db = db;
        _log = log;
    }

    public async Task RecordAsync(long viewerId, BattleContext? ctx, bool isWin, CancellationToken ct)
    {
        if (ctx is null)
        {
            _log.LogWarning(
                "BattleHistoryWriter.RecordAsync called with null context for viewer {ViewerId} - " +
                "likely missed start-time Set (server restart or non-tracked family). Skipping.",
                viewerId);
            return;
        }

        var existing = await _db.ViewerBattleHistories
            .AnyAsync(h => h.ViewerId == viewerId && h.BattleId == ctx.BattleId, ct);
        if (existing) return; // idempotent

        var count = await _db.ViewerBattleHistories
            .CountAsync(h => h.ViewerId == viewerId, ct);
        if (count >= RetentionCap)
        {
            var oldest = await _db.ViewerBattleHistories
                .Where(h => h.ViewerId == viewerId)
                .OrderBy(h => h.CreateTime)
                .FirstAsync(ct);
            _db.ViewerBattleHistories.Remove(oldest);
        }

        _db.ViewerBattleHistories.Add(new ViewerBattleHistory
        {
            ViewerId            = viewerId,
            BattleId            = ctx.BattleId,
            BattleType          = ctx.BattleType,
            DeckFormat          = ctx.DeckFormat,
            TwoPickType         = ctx.TwoPickType,
            IsLimitTurn         = 0,
            SelfClassId         = ctx.SelfClassId,
            SelfSubClassId      = ctx.SelfSubClassId,
            SelfCharaId         = ctx.SelfCharaId,
            SelfRotationId      = ctx.SelfRotationId,
            OpponentClassId     = ctx.OpponentClassId,
            OpponentSubClassId  = ctx.OpponentSubClassId,
            OpponentCharaId     = ctx.OpponentCharaId,
            OpponentName        = ctx.OpponentName,
            OpponentCountryCode = ctx.OpponentCountryCode,
            OpponentEmblemId    = ctx.OpponentEmblemId,
            OpponentDegreeId    = ctx.OpponentDegreeId,
            OpponentRotationId  = ctx.OpponentRotationId,
            IsWin               = isWin,
            BattleStartTime     = ctx.BattleStartTime,
            CreateTime          = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
    }
}
