using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Persistence;

public class ViewerBattleHistoryPersistenceTests
{
    [Test]
    public async Task ViewerBattleHistory_round_trips_composite_PK_row()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattleHistories.Add(new ViewerBattleHistory
            {
                ViewerId = viewerId,
                BattleId = 234_471_983_876L,
                BattleType = 4,
                DeckFormat = 2,
                TwoPickType = 0,
                IsLimitTurn = 0,
                SelfClassId = 8,
                SelfSubClassId = 0,
                SelfCharaId = 8,
                SelfRotationId = "0",
                OpponentClassId = 5,
                OpponentSubClassId = 0,
                OpponentCharaId = 805,
                OpponentName = "Foo",
                OpponentCountryCode = "",
                OpponentEmblemId = 721_341_010L,
                OpponentDegreeId = 120_023L,
                OpponentRotationId = "0",
                IsWin = false,
                BattleStartTime = new DateTime(2026, 6, 4, 17, 13, 13, DateTimeKind.Utc),
                CreateTime = new DateTime(2026, 6, 4, 17, 16, 6, DateTimeKind.Utc),
            });
            await db.SaveChangesAsync();
        }

        using (var readScope = factory.Services.CreateScope())
        {
            var db = readScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var row = await db.ViewerBattleHistories.SingleAsync(h =>
                h.ViewerId == viewerId && h.BattleId == 234_471_983_876L);
            Assert.That(row.OpponentName, Is.EqualTo("Foo"));
            Assert.That(row.OpponentEmblemId, Is.EqualTo(721_341_010L));
        }
    }

    [Test]
    public async Task ViewerBattleHistory_composite_PK_rejects_duplicate_battle_id_for_same_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattleHistories.Add(NewRow(viewerId, battleId: 1L));
            await db.SaveChangesAsync();
        }

        using (var dupScope = factory.Services.CreateScope())
        {
            var db = dupScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerBattleHistories.Add(NewRow(viewerId, battleId: 1L));
            Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
        }
    }

    private static ViewerBattleHistory NewRow(long viewerId, long battleId) => new()
    {
        ViewerId = viewerId,
        BattleId = battleId,
        SelfRotationId = "0",
        OpponentName = "",
        OpponentCountryCode = "",
        OpponentRotationId = "0",
        BattleStartTime = DateTime.UtcNow,
        CreateTime = DateTime.UtcNow,
    };
}
