using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class GachaPointServiceTests
{
    [Test]
    public async Task GetRewards_returns_empty_when_pack_has_no_gacha_point_config()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10001, BasePackId = 10001, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = null,
        });
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10001, viewerId);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetRewards_emits_standard_legendaries_with_emblem_reward()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Seed: a card set, two legendary cards in it (class 0/neutral and class 1/forest),
        // and a bronze card to confirm the rarity filter. Neutral cards have Class = null
        // (per ShadowverseCardEntry.Class XML doc); Forestcraft (id=1) is already seeded by
        // the ReferenceDataImporter, so we look it up rather than re-insert.
        var classForest = await db.Classes.FirstAsync(c => c.Id == 1);

        var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
        db.CardSets.Add(set);

        var legNeutral = new ShadowverseCardEntry
        {
            Id = 108041010, Name = "leg-neutral", Rarity = Rarity.Legendary,
            Class = null, IsFoil = false,
        };
        var legForest = new ShadowverseCardEntry
        {
            Id = 108141010, Name = "leg-forest", Rarity = Rarity.Legendary,
            Class = classForest, IsFoil = false,
        };
        var bronze = new ShadowverseCardEntry
        {
            Id = 108041020, Name = "bronze-neutral", Rarity = Rarity.Bronze,
            Class = null, IsFoil = false,
        };
        set.Cards.AddRange(new[] { legNeutral, legForest, bronze });

        db.CardCosmeticRewards.AddRange(
            new CardCosmeticReward { CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100 },
            new CardCosmeticReward { CardId = 108141010, Type = CosmeticType.Emblem, CosmeticId = 1081410100 });

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
        });
        await db.SaveChangesAsync();
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10008, viewerId);

        Assert.That(result, Has.Count.EqualTo(2));
        var first = result[0];
        Assert.That(first.ClassId, Is.EqualTo("0"));
        Assert.That(first.CardId, Is.EqualTo(108041010));
        Assert.That(first.IsReceived, Is.False);
        Assert.That(first.RewardList, Has.Count.EqualTo(1));
        Assert.That(first.RewardList[0].RewardType, Is.EqualTo(7));        // Emblem
        Assert.That(first.RewardList[0].RewardDetailId, Is.EqualTo(1080410100));
        Assert.That(first.RewardList[0].RewardNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task GetRewards_emits_leader_cards_with_three_reward_entries()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var classForest = await db.Classes.FirstAsync(c => c.Id == 1);

        var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
        db.CardSets.Add(set);

        // Leader card in pool — identified by presence of a Type=Skin cosmetic reward.
        var leader = new ShadowverseCardEntry
        {
            Id = 704141010, Name = "leader-forest", Rarity = Rarity.Legendary,
            Class = classForest, IsFoil = false,
        };
        set.Cards.Add(leader);

        db.CardCosmeticRewards.AddRange(
            new CardCosmeticReward { CardId = 704141010, Type = CosmeticType.Skin, CosmeticId = 401 },
            new CardCosmeticReward { CardId = 704141010, Type = CosmeticType.Emblem, CosmeticId = 704141010 });

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
        });
        await db.SaveChangesAsync();
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10008, viewerId);

        Assert.That(result, Has.Count.EqualTo(1));
        var leaderEntry = result[0];
        Assert.That(leaderEntry.CardId, Is.EqualTo(704141010));
        Assert.That(leaderEntry.RewardList, Has.Count.EqualTo(3));

        // Order verified against prod capture: type=6 (Sleeve in enum, "Card cosmetic" in this
        // context), type=10 (Skin), type=7 (Emblem).
        Assert.That(leaderEntry.RewardList[0].RewardType, Is.EqualTo(6));
        Assert.That(leaderEntry.RewardList[0].RewardDetailId, Is.EqualTo(704141010));
        Assert.That(leaderEntry.RewardList[1].RewardType, Is.EqualTo(10));
        Assert.That(leaderEntry.RewardList[1].RewardDetailId, Is.EqualTo(401));
        Assert.That(leaderEntry.RewardList[2].RewardType, Is.EqualTo(7));
        Assert.That(leaderEntry.RewardList[2].RewardDetailId, Is.EqualTo(704141010));
    }

    [Test]
    public async Task GetRewards_emits_one_entry_per_emblem_for_cards_with_multiple_emblems()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
        db.CardSets.Add(set);
        var leg = new ShadowverseCardEntry
        {
            Id = 108044010, Name = "leg-multi-emblem", Rarity = Rarity.Legendary,
            Class = null, IsFoil = false,
        };
        set.Cards.Add(leg);

        // Two emblems for one card — matches prod capture pack 10008 card 108044010.
        db.CardCosmeticRewards.AddRange(
            new CardCosmeticReward { CardId = 108044010, Type = CosmeticType.Emblem, CosmeticId = 900041040 },
            new CardCosmeticReward { CardId = 108044010, Type = CosmeticType.Emblem, CosmeticId = 900041050 });

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
        });
        await db.SaveChangesAsync();
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10008, viewerId);

        Assert.That(result, Has.Count.EqualTo(1));
        var entry = result[0];
        Assert.That(entry.CardId, Is.EqualTo(108044010));
        Assert.That(entry.RewardList, Has.Count.EqualTo(2),
            "cards with multiple emblems must emit one reward_list entry per emblem");
        var detailIds = entry.RewardList.Select(r => r.RewardDetailId).OrderBy(x => x).ToList();
        Assert.That(detailIds, Is.EqualTo(new[] { 900041040L, 900041050L }));
        Assert.That(entry.RewardList.All(r => r.RewardType == 7), Is.True);
    }

    [Test]
    public async Task GetRewards_marks_already_received_cards()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
        db.CardSets.Add(set);
        var leg = new ShadowverseCardEntry
        {
            Id = 108041010, Name = "leg", Rarity = Rarity.Legendary,
            Class = null, IsFoil = false,
        };
        set.Cards.Add(leg);
        db.CardCosmeticRewards.Add(new CardCosmeticReward
        {
            CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100,
        });
        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
        });
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.GachaPointReceived.Add(new ViewerGachaPointReceived
        {
            PackId = 10008, CardId = 108041010, ReceivedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10008, viewerId);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].IsReceived, Is.True);
    }

    [Test]
    public async Task Accrue_uses_pack_increase_when_child_override_is_zero()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            ChildGachas =
            {
                new PackChildGachaEntry
                {
                    GachaId = 100081, TypeDetail = CardPackType.CrystalMulti, Cost = 100, CardCount = 8,
                    OverrideIncreaseGachaPoint = 0,
                },
            },
        });
        await db.SaveChangesAsync();

        var viewer = await db.Viewers
            .Include(v => v.GachaPointBalances)
            .FirstAsync(v => v.Id == viewerId);
        var pack = await db.Packs.Include(p => p.ChildGachas).FirstAsync(p => p.Id == 10008);
        var child = pack.ChildGachas[0];

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        svc.Accrue(viewer, pack, child, packNumber: 10);
        await db.SaveChangesAsync();

        var balance = viewer.GachaPointBalances.Single(b => b.PackId == 10008);
        Assert.That(balance.Points, Is.EqualTo(10));
    }

    [Test]
    public async Task Accrue_child_override_takes_precedence_over_pack_increase()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            ChildGachas =
            {
                new PackChildGachaEntry
                {
                    GachaId = 100085, TypeDetail = CardPackType.TicketMulti, Cost = 0, CardCount = 8,
                    OverrideIncreaseGachaPoint = 3,
                },
            },
        });
        await db.SaveChangesAsync();

        var viewer = await db.Viewers.Include(v => v.GachaPointBalances).FirstAsync(v => v.Id == viewerId);
        var pack = await db.Packs.Include(p => p.ChildGachas).FirstAsync(p => p.Id == 10008);
        var child = pack.ChildGachas[0];

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        svc.Accrue(viewer, pack, child, packNumber: 2);
        await db.SaveChangesAsync();

        Assert.That(viewer.GachaPointBalances.Single().Points, Is.EqualTo(6));
    }

    [Test]
    public async Task Accrue_is_noop_when_pack_has_no_gacha_point_config()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.Packs.Add(new PackConfigEntry
        {
            Id = 99047, BasePackId = 99047, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = null,
            ChildGachas = { new PackChildGachaEntry { GachaId = 990475, TypeDetail = CardPackType.TicketMulti, Cost = 0, CardCount = 8 } },
        });
        await db.SaveChangesAsync();

        var viewer = await db.Viewers.Include(v => v.GachaPointBalances).FirstAsync(v => v.Id == viewerId);
        var pack = await db.Packs.Include(p => p.ChildGachas).FirstAsync(p => p.Id == 99047);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        svc.Accrue(viewer, pack, pack.ChildGachas[0], packNumber: 5);
        await db.SaveChangesAsync();

        Assert.That(viewer.GachaPointBalances, Is.Empty);
    }

    [Test]
    public async Task Accrue_increments_existing_balance_on_second_call()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
            ChildGachas =
            {
                new PackChildGachaEntry { GachaId = 100087, TypeDetail = CardPackType.RupyMulti, Cost = 100, CardCount = 8 },
            },
        });
        await db.SaveChangesAsync();

        var viewer = await db.Viewers.Include(v => v.GachaPointBalances).FirstAsync(v => v.Id == viewerId);
        var pack = await db.Packs.Include(p => p.ChildGachas).FirstAsync(p => p.Id == 10008);
        var child = pack.ChildGachas[0];

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();

        // First accrual: creates the balance row.
        svc.Accrue(viewer, pack, child, packNumber: 3);
        await db.SaveChangesAsync();

        // Second accrual: must hit the existing row (the `+=` branch), not create a duplicate.
        svc.Accrue(viewer, pack, child, packNumber: 5);
        await db.SaveChangesAsync();

        Assert.That(viewer.GachaPointBalances, Has.Count.EqualTo(1),
            "second Accrue must update the existing row, not create a duplicate");
        Assert.That(viewer.GachaPointBalances.Single().Points, Is.EqualTo(8));
    }

    [Test]
    public async Task TryExchange_fails_when_balance_below_threshold()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        SeedPackWithOneLegendary(db, packId: 10008, threshold: 400);

        var viewer = await db.Viewers.Include(v => v.GachaPointBalances).FirstAsync(v => v.Id == viewerId);
        viewer.GachaPointBalances.Add(new ViewerGachaPointBalance { PackId = 10008, Points = 399 });
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId, configure: cfg => cfg
            .WithInclude(v => v.GachaPointBalances)
            .WithInclude(v => v.GachaPointReceived));

        var outcome = await svc.TryExchangeAsync(tx, 10008, 108041010);

        Assert.That(outcome.Success, Is.False);
        Assert.That(outcome.Error, Is.EqualTo("insufficient_gacha_points"));
    }

    [Test]
    public async Task TryExchange_fails_when_card_not_in_pack_catalog()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        SeedPackWithOneLegendary(db, packId: 10008, threshold: 400);

        var viewer = await db.Viewers.Include(v => v.GachaPointBalances).FirstAsync(v => v.Id == viewerId);
        viewer.GachaPointBalances.Add(new ViewerGachaPointBalance { PackId = 10008, Points = 400 });
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId, configure: cfg => cfg
            .WithInclude(v => v.GachaPointBalances)
            .WithInclude(v => v.GachaPointReceived));

        var outcome = await svc.TryExchangeAsync(tx, 10008, cardId: 999999999);  // not in pool

        Assert.That(outcome.Success, Is.False);
        Assert.That(outcome.Error, Is.EqualTo("card_not_exchangeable"));
    }

    [Test]
    public async Task TryExchange_fails_when_card_already_received()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        SeedPackWithOneLegendary(db, packId: 10008, threshold: 400);

        var viewer = await db.Viewers
            .Include(v => v.GachaPointBalances)
            .Include(v => v.GachaPointReceived)
            .FirstAsync(v => v.Id == viewerId);
        viewer.GachaPointBalances.Add(new ViewerGachaPointBalance { PackId = 10008, Points = 400 });
        viewer.GachaPointReceived.Add(new ViewerGachaPointReceived
        {
            PackId = 10008, CardId = 108041010, ReceivedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId, configure: cfg => cfg
            .WithInclude(v => v.GachaPointBalances)
            .WithInclude(v => v.GachaPointReceived));

        var outcome = await svc.TryExchangeAsync(tx, 10008, 108041010);

        Assert.That(outcome.Success, Is.False);
        Assert.That(outcome.Error, Is.EqualTo("already_received"));
    }

    [Test]
    public async Task TryExchange_succeeds_debits_marks_received_and_returns_grants()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        SeedPackWithOneLegendary(db, packId: 10008, threshold: 400);

        var preViewer = await db.Viewers
            .Include(v => v.GachaPointBalances)
            .FirstAsync(v => v.Id == viewerId);
        preViewer.GachaPointBalances.Add(new ViewerGachaPointBalance { PackId = 10008, Points = 500 });
        await db.SaveChangesAsync();

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId, configure: cfg => cfg
            .WithInclude(v => v.GachaPointBalances)
            .WithInclude(v => v.GachaPointReceived));

        var outcome = await svc.TryExchangeAsync(tx, 10008, 108041010);
        Assert.That(outcome.Success, Is.True);

        await tx.CommitAsync();

        // Balance debited (check via tx.Viewer which is tracked).
        Assert.That(tx.Viewer.GachaPointBalances.Single().Points, Is.EqualTo(100));

        // Marker written.
        Assert.That(tx.Viewer.GachaPointReceived
            .Any(r => r.PackId == 10008 && r.CardId == 108041010), Is.True);

        // Reward list non-empty: at minimum the card grant.
        Assert.That(outcome.RewardList, Is.Not.Empty);
        Assert.That(outcome.RewardList.Any(r => r.RewardType == (int)UserGoodsType.Card && r.RewardId == 108041010),
            Is.True, "card grant missing");
    }

    [Test]
    public async Task GetRewards_emits_correct_class_id_for_non_neutral_cards()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Use the pre-seeded Forestcraft (Id=1) and Havencraft (Id=7) classes from ReferenceDataImporter.
        var forest = await db.Classes.FirstAsync(c => c.Id == 1);
        var haven = await db.Classes.FirstAsync(c => c.Id == 7);

        var set = new ShadowverseCardSetEntry { Id = 10008, IsInRotation = true };
        db.CardSets.Add(set);
        set.Cards.AddRange(new[]
        {
            new ShadowverseCardEntry
            {
                Id = 108141010, Name = "leg-forest", Rarity = Rarity.Legendary,
                Class = forest, IsFoil = false,
            },
            new ShadowverseCardEntry
            {
                Id = 108741010, Name = "leg-haven", Rarity = Rarity.Legendary,
                Class = haven, IsFoil = false,
            },
        });
        db.CardCosmeticRewards.AddRange(
            new CardCosmeticReward { CardId = 108141010, Type = CosmeticType.Emblem, CosmeticId = 1081410100 },
            new CardCosmeticReward { CardId = 108741010, Type = CosmeticType.Emblem, CosmeticId = 1087410100 });
        db.Packs.Add(new PackConfigEntry
        {
            Id = 10008, BasePackId = 10008, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
        });
        await db.SaveChangesAsync();
        await factory.SeedPackDrawTableFromSetAsync(10008, 10008);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10008, viewerId);

        // Cards must surface with their REAL class id, not the "0" neutral fallback caused
        // by the pool provider not Including Class.
        Assert.That(result, Has.Count.EqualTo(2));
        var forestEntry = result.Single(r => r.CardId == 108141010);
        var havenEntry = result.Single(r => r.CardId == 108741010);
        Assert.That(forestEntry.ClassId, Is.EqualTo("1"));
        Assert.That(havenEntry.ClassId, Is.EqualTo("7"));
    }

    [Test]
    public async Task GetRewards_includes_legendaries_without_emblem_cosmetics()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Pack 10001 simulation: a legendary with NO CardCosmeticReward rows at all.
        // Must still appear in the catalog (per user spec — all pack legendaries are
        // exchangeable, regardless of whether we have captured emblem mappings).
        // Use a unique card-set id (10099) because SVSimTestFactory already seeds a
        // minimal CardSet at Id=10001.
        var set = new ShadowverseCardSetEntry { Id = 10099, IsInRotation = true };
        db.CardSets.Add(set);
        set.Cards.Add(new ShadowverseCardEntry
        {
            Id = 101141020, Name = "old-leg", Rarity = Rarity.Legendary,
            Class = null, IsFoil = false,
        });
        db.Packs.Add(new PackConfigEntry
        {
            Id = 10099, BasePackId = 10099, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = 400, IncreaseGachaPoint = 1 },
        });
        await db.SaveChangesAsync();
        await factory.SeedPackDrawTableFromSetAsync(10099, 10099);

        var svc = scope.ServiceProvider.GetRequiredService<IGachaPointService>();
        var result = await svc.GetRewardsAsync(10099, viewerId);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].CardId, Is.EqualTo(101141020));
        Assert.That(result[0].RewardList, Is.Empty,
            "legendary without emblem rows emits empty reward_list — the catalog declaration is just the card itself");
    }

    private static void SeedPackWithOneLegendary(SVSimDbContext db, int packId, int threshold)
    {
        var cls = db.Classes.Find(0) ?? new ClassEntry { Id = 0, Name = "Neutral" };
        if (db.Classes.Find(0) is null) db.Classes.Add(cls);
        var set = new ShadowverseCardSetEntry { Id = packId, IsInRotation = true };
        db.CardSets.Add(set);
        set.Cards.Add(new ShadowverseCardEntry
        {
            Id = 108041010, Name = "leg", Rarity = Rarity.Legendary,
            Class = cls, IsFoil = false,
        });
        db.CardCosmeticRewards.Add(new CardCosmeticReward
        {
            CardId = 108041010, Type = CosmeticType.Emblem, CosmeticId = 1080410100,
        });
        db.Packs.Add(new PackConfigEntry
        {
            Id = packId, BasePackId = packId, PackCategory = PackCategory.LegendCardPack,
            CommenceDate = DateTime.UtcNow.AddDays(-1), CompleteDate = DateTime.UtcNow.AddDays(30),
            GachaPointConfig = new PackGachaPointConfig { ExchangeablePoint = threshold, IncreaseGachaPoint = 1 },
        });
        // Draw table pointing at the seeded legendary so IPackDrawTableRepository.GetAsync
        // surfaces it for GachaPointService.GetRewardsAsync / TryExchangeAsync.
        db.PackDrawConfigs.Add(new PackDrawConfigEntry { Id = packId, AnimationRatePct = 0 });
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.General, Tier = DrawTier.Legendary, RatePct = 100 });
        db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry
        {
            PackId = packId, Slot = DrawSlot.General, Tier = DrawTier.Legendary, CardId = 108041010, RatePct = 100,
        });
        db.SaveChanges();
    }
}
