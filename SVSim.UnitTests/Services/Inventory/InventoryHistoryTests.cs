using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database.Services.Inventory;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryHistoryTests
{
    [Test]
    public void GrantSourceMessages_returns_known_messages_for_seeded_sources()
    {
        Assert.That(GrantSourceMessages.For(GrantSource.DailyBonus), Is.EqualTo("Daily Bonus"));
        Assert.That(GrantSourceMessages.For(GrantSource.PackOpen), Is.EqualTo("From buying card packs"));
        Assert.That(GrantSourceMessages.For(GrantSource.CardCosmeticCascade), Is.EqualTo("Card cosmetic"));
        Assert.That(GrantSourceMessages.For(GrantSource.Unknown), Is.EqualTo("Unknown"));
    }

    [Test]
    public void GrantSourceMessages_covers_every_enum_value()
    {
        foreach (GrantSource source in Enum.GetValues<GrantSource>())
        {
            var message = GrantSourceMessages.For(source);
            Assert.That(message, Is.Not.Null.And.Not.Empty,
                $"GrantSource.{source} has no message defined.");
        }
    }

    [Test]
    public void InventoryLoadConfig_Source_defaults_to_Unknown()
    {
        var cfg = new InventoryLoadConfig();
        Assert.That(cfg.Source, Is.EqualTo(GrantSource.Unknown));
    }

    [Test]
    public void InventoryLoadConfig_Source_is_assignable()
    {
        var cfg = new InventoryLoadConfig { Source = GrantSource.PackOpen };
        Assert.That(cfg.Source, Is.EqualTo(GrantSource.PackOpen));
    }

    [Test]
    public async Task ViewerAcquireHistory_DbSet_round_trips_a_row()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();

        ctx.ViewerAcquireHistory.Add(new SVSim.Database.Models.ViewerAcquireHistoryEntry
        {
            ViewerId = viewerId,
            RewardType = (int)SVSim.Database.Enums.UserGoodsType.Rupy,
            RewardDetailId = 0,
            RewardCount = 50,
            AcquireType = (int)GrantSource.DailyBonus,
            Message = "Daily Bonus",
            AcquireTime = new DateTime(2026, 6, 9, 12, 0, 0, DateTimeKind.Utc),
        });
        await ctx.SaveChangesAsync();

        var roundtrip = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId).ToListAsync();
        Assert.That(roundtrip, Has.Count.EqualTo(1));
        Assert.That(roundtrip[0].RewardCount, Is.EqualTo(50));
        Assert.That(roundtrip[0].AcquireType, Is.EqualTo((int)GrantSource.DailyBonus));
    }

    [Test]
    public async Task Commit_writes_one_history_row_per_grant_tagged_with_source()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();

        DateTime before = DateTime.UtcNow;
        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.DailyBonus))
        {
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Rupy, 0, 20);
            await tx.CommitAsync();
        }
        DateTime after = DateTime.UtcNow;

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId).ToListAsync();

        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].RewardType, Is.EqualTo((int)SVSim.Database.Enums.UserGoodsType.Rupy));
        Assert.That(rows[0].RewardDetailId, Is.EqualTo(0));
        Assert.That(rows[0].RewardCount, Is.EqualTo(20));
        Assert.That(rows[0].AcquireType, Is.EqualTo((int)GrantSource.DailyBonus));
        Assert.That(rows[0].Message, Is.EqualTo("Daily Bonus"));
        Assert.That(rows[0].AcquireTime, Is.InRange(before, after));
    }

    [Test]
    public async Task Commit_writes_multiple_rows_in_order_with_shared_timestamp()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();

        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.PackOpen))
        {
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Rupy, 0, 1);
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Crystal, 0, 2);
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.RedEther, 0, 3);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId)
            .OrderBy(h => h.Id)
            .ToListAsync();

        Assert.That(rows, Has.Count.EqualTo(3));
        Assert.That(rows.Select(r => r.RewardCount), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(rows.Select(r => r.AcquireTime).Distinct().Count(), Is.EqualTo(1),
            "all rows in one commit share AcquireTime");
        Assert.That(rows.All(r => r.AcquireType == (int)GrantSource.PackOpen), Is.True);
        Assert.That(rows.All(r => r.Message == "From buying card packs"), Is.True);
    }

    [Test]
    public async Task Commit_writes_no_history_rows_for_spend_only_transactions()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var seedScope = factory.Services.CreateScope();
        var seedCtx = seedScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var v = await seedCtx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 1000;
        await seedCtx.SaveChangesAsync();

        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();
        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.ItemPurchase))
        {
            await tx.TrySpendAsync(SVSim.Database.Services.SpendCurrency.Crystal, 500);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId).ToListAsync();
        Assert.That(rows, Is.Empty);
    }

    [Test]
    public async Task Commit_writes_cascade_cosmetic_with_distinct_source_and_message()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        const int sleeveId = 2_000_030_001;
        ctx.Sleeves.Add(new SVSim.Database.Models.SleeveEntry { Id = sleeveId });
        ctx.CardCosmeticRewards.Add(new SVSim.Database.Models.CardCosmeticReward
        {
            CardId = (int)cardId,
            CosmeticId = sleeveId,
            Type = SVSim.Database.Enums.CosmeticType.Sleeve,
        });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();
        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.PackOpen))
        {
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Card, cardId, 1);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx2.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId)
            .OrderBy(h => h.Id)
            .ToListAsync();

        Assert.That(rows, Has.Count.EqualTo(2), "card grant + cascade sleeve grant");
        Assert.That(rows[0].RewardType, Is.EqualTo((int)SVSim.Database.Enums.UserGoodsType.Card));
        Assert.That(rows[0].AcquireType, Is.EqualTo((int)GrantSource.PackOpen));
        Assert.That(rows[0].Message, Is.EqualTo("From buying card packs"));
        Assert.That(rows[1].RewardType, Is.EqualTo((int)SVSim.Database.Enums.UserGoodsType.Sleeve));
        Assert.That(rows[1].AcquireType, Is.EqualTo((int)GrantSource.CardCosmeticCascade));
        Assert.That(rows[1].Message, Is.EqualTo("Card cosmetic"));
    }

    [Test]
    public async Task Commit_writes_no_history_row_for_item_debit()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        // Seed an item the viewer owns so DebitItem has something to spend.
        const int itemId = 5550001;
        await factory.SeedOwnedItemAsync(viewerId, itemId, 5);

        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();
        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.ItemPurchase))
        {
            await tx.TryDebitAsync(SVSim.Database.Enums.UserGoodsType.Item, itemId, 1);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx2.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId).ToListAsync();
        Assert.That(rows, Is.Empty, "item debit should not produce a history row");
    }

    [Test]
    public async Task Commit_zero_pads_detail_id_for_wallet_currencies()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();

        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.DailyBonus))
        {
            // detailId=99 is meaningful for some types but ignored for wallets — should still write 0.
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Crystal, 99, 5);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var row = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId).FirstAsync();
        Assert.That(row.RewardDetailId, Is.EqualTo(0));
    }

    [Test]
    public async Task PackOpen_path_appends_to_acquire_history_when_source_is_set()
    {
        // Direct exercise of the GrantSource plumbing as a stand-in for the real /pack/open route.
        // After Task 11 migrates PackController.Open, a higher-level E2E test could replace this;
        // for now this asserts the integration shape Pack open will fall into.
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();

        await using (var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.PackOpen))
        {
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Card, cardId, 1);
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Card, cardId, 1);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId)
            .OrderBy(h => h.Id)
            .ToListAsync();
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows.All(r => r.AcquireType == (int)GrantSource.PackOpen), Is.True);
        Assert.That(rows.All(r => r.Message == "From buying card packs"), Is.True);
    }

    [Test]
    public async Task Commit_prunes_history_above_retention_cap_per_viewer()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);
        long otherViewerId = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_002UL);
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();

        // Pre-seed 305 rows for the primary viewer via 305 single-grant commits.
        for (int i = 0; i < 305; i++)
        {
            await using var tx = await inv.BeginAsync(viewerId, configure: c => c.Source = GrantSource.DailyBonus);
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Rupy, 0, 1);
            await tx.CommitAsync();
        }

        // Seed 50 rows for an unrelated viewer to verify pruning is per-viewer.
        for (int i = 0; i < 50; i++)
        {
            await using var tx = await inv.BeginAsync(otherViewerId, configure: c => c.Source = GrantSource.DailyBonus);
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Rupy, 0, 1);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var primaryCount = await ctx.ViewerAcquireHistory.CountAsync(h => h.ViewerId == viewerId);
        var otherCount = await ctx.ViewerAcquireHistory.CountAsync(h => h.ViewerId == otherViewerId);

        Assert.That(primaryCount, Is.EqualTo(300), "primary viewer pruned to cap");
        Assert.That(otherCount, Is.EqualTo(50), "other viewer untouched by primary's prune");
    }

    [Test]
    public async Task Commit_writes_history_row_with_Unknown_source_when_caller_omits_Source()
    {
        using var factory = new SVSim.UnitTests.Infrastructure.SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.Inventory.IInventoryService>();

        // No configure callback — Source defaults to Unknown.
        await using (var tx = await inv.BeginAsync(viewerId))
        {
            await tx.GrantAsync(SVSim.Database.Enums.UserGoodsType.Rupy, 0, 10);
            await tx.CommitAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSim.Database.SVSimDbContext>();
        var rows = await ctx.ViewerAcquireHistory.AsNoTracking()
            .Where(h => h.ViewerId == viewerId).ToListAsync();
        Assert.That(rows, Has.Count.EqualTo(1));
        Assert.That(rows[0].AcquireType, Is.EqualTo((int)GrantSource.Unknown));
        Assert.That(rows[0].Message, Is.EqualTo("Unknown"));
    }
}
