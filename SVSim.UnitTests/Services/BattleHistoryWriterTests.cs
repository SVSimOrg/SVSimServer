using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Services.Replay;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class BattleHistoryWriterTests
{
    private static BattleContext MakeCtx(long battleId) => new(
        BattleId: battleId,
        BattleType: 2, DeckFormat: 0, TwoPickType: 0,
        SelfClassId: 1, SelfSubClassId: 0, SelfCharaId: 1, SelfRotationId: "0",
        OpponentViewerId: 0, OpponentName: "Bot", OpponentClassId: 2,
        OpponentSubClassId: 0, OpponentCharaId: 1, OpponentCountryCode: "",
        OpponentEmblemId: 0, OpponentDegreeId: 0, OpponentRotationId: "0",
        BattleStartTime: DateTime.UtcNow);

    [Test]
    public async Task RecordAsync_writes_row()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using var scope = factory.Services.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IBattleHistoryWriter>();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await writer.RecordAsync(viewerId, MakeCtx(battleId: 42), isWin: true, default);

        var row = await db.ViewerBattleHistories
            .SingleAsync(h => h.ViewerId == viewerId && h.BattleId == 42);
        Assert.That(row.IsWin, Is.True);
        Assert.That(row.OpponentName, Is.EqualTo("Bot"));
    }

    [Test]
    public async Task RecordAsync_null_ctx_is_noop()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using var scope = factory.Services.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IBattleHistoryWriter>();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await writer.RecordAsync(viewerId, ctx: null, isWin: true, default);

        Assert.That(await db.ViewerBattleHistories.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RecordAsync_duplicate_battle_id_skips_silently()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using var scope = factory.Services.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IBattleHistoryWriter>();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        await writer.RecordAsync(viewerId, MakeCtx(battleId: 42), isWin: true, default);
        await writer.RecordAsync(viewerId, MakeCtx(battleId: 42), isWin: false, default);

        // Original row is preserved; the second call is a no-op.
        var row = await db.ViewerBattleHistories
            .SingleAsync(h => h.ViewerId == viewerId && h.BattleId == 42);
        Assert.That(row.IsWin, Is.True);
    }

    [Test]
    public async Task RecordAsync_evicts_oldest_when_over_retention_cap()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using var scope = factory.Services.CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<IBattleHistoryWriter>();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Write 51 rows. The first (oldest CreateTime) should be evicted on row 51's insert.
        for (long i = 1; i <= 51; i++)
        {
            await writer.RecordAsync(viewerId, MakeCtx(battleId: i), isWin: false, default);
        }

        var count = await db.ViewerBattleHistories.CountAsync(h => h.ViewerId == viewerId);
        Assert.That(count, Is.EqualTo(50));

        var hasOldest = await db.ViewerBattleHistories
            .AnyAsync(h => h.ViewerId == viewerId && h.BattleId == 1);
        Assert.That(hasOldest, Is.False, "battle_id=1 should have been evicted as the oldest");
    }
}
