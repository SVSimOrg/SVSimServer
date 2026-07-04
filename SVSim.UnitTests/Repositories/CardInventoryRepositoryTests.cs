using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Card;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

/// <summary>
/// Coverage for <c>CardInventoryRepository.DestructCards</c>. Exercises the validate→mutate
/// loop directly so tests don't need to round-trip through HTTP; the controller-level wire
/// behavior is covered separately in <c>CardControllerTests</c>.
/// </summary>
public class CardInventoryRepositoryTests
{
    [Test]
    public async Task Destruct_single_card_decrements_count_and_awards_vials()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 5, dustReward: 50);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.True, outcome.Error?.ToString());
        Assert.That(outcome.Result!.NewRedEtherTotal, Is.EqualTo(50UL));
        Assert.That(outcome.Result!.NewOwnedCounts[10001001L], Is.EqualTo(4));

        // Verify persisted state matches what was returned
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(50UL));
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(4));
    }

    [Test]
    public async Task Destruct_batch_decrements_each_card_and_sums_vials()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 50);
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001002L, count: 2, dustReward: 200);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int>
        {
            { 10001001L, 2 },
            { 10001002L, 1 },
        });

        Assert.That(outcome.IsSuccess, Is.True);
        // 2 * 50 + 1 * 200 = 300
        Assert.That(outcome.Result!.NewRedEtherTotal, Is.EqualTo(300UL));
        Assert.That(outcome.Result!.NewOwnedCounts[10001001L], Is.EqualTo(1));
        Assert.That(outcome.Result!.NewOwnedCounts[10001002L], Is.EqualTo(1));
    }

    [Test]
    public async Task Destruct_leaves_zero_count_row_after_destructing_last_copy()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // We just verify the OwnedCardEntry row survives a destruct-to-zero, so future operations (re-protect, re-craft) can attach to it.
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 1, dustReward: 50);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        var owned = viewer.Cards.FirstOrDefault(c => c.Card.Id == 10001001L);
        Assert.That(owned, Is.Not.Null, "OwnedCardEntry row should remain after destruct-to-zero");
        Assert.That(owned!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Destruct_rejects_unknown_card_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 5, dustReward: 50);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 99_999_999L, 1 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(DestructError.UnknownCard));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(0UL));
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(5));
    }

    [Test]
    public async Task Destruct_rejects_insufficient_count_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, dustReward: 50);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 3 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(DestructError.InsufficientCards));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(0UL));
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Destruct_rejects_protected_card_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 50, isProtected: true);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(DestructError.CardProtected));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(0UL));
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Destruct_rejects_non_destructible_card_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // dustReward=0 marks a card as IsNotCraftDestruct (e.g. tokens, basics)
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 0, craftCost: 0);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(DestructError.NotDestructible));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Destruct_validates_full_batch_before_mutating()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 5, dustReward: 50);
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001002L, count: 3, dustReward: 200, isProtected: true);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int>
        {
            { 10001001L, 2 },   // would-be valid
            { 10001002L, 1 },   // protected — fails validation
        });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(DestructError.CardProtected));

        // Critical: the valid card must be untouched. Proves validation runs against the full
        // batch before any inventory write.
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(0UL), "no vials awarded when batch fails");
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(5));
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001002L).Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Destruct_strips_excess_copies_from_a_deck()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 50);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 1);
        await factory.AddCardToDeckAsync(viewerId, Format.Rotation, deckNumber: 1, cardId: 10001001L, count: 3);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        // Destruct 2 — owned drops from 3 to 1 — deck must lose 2 of the 3 copies it had.
        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 2 } });
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var deck = await db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Decks)
            .Include(d => d.Cards).ThenInclude(c => c.Card)
            .FirstAsync(d => d.Format == Format.Rotation && d.Number == 1);

        var deckCard = deck.Cards.First(c => c.Card.Id == 10001001L);
        Assert.That(deckCard.Count, Is.EqualTo(1), "deck should now hold only 1 copy of the card");
    }

    [Test]
    public async Task Destruct_strips_excess_across_multiple_decks()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 3, dustReward: 50);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 1);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 2);
        await factory.AddCardToDeckAsync(viewerId, Format.Rotation, 1, cardId: 10001001L, count: 3);
        await factory.AddCardToDeckAsync(viewerId, Format.Rotation, 2, cardId: 10001001L, count: 3);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        // Destruct 1 — owned drops from 3 to 2 — each deck must lose 1 copy.
        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var decks = await db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Decks)
            .Include(d => d.Cards).ThenInclude(c => c.Card)
            .Where(d => d.Format == Format.Rotation)
            .ToListAsync();

        foreach (var deck in decks)
        {
            Assert.That(deck.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(2),
                $"deck {deck.Number} should now hold 2 copies");
        }
    }

    [Test]
    public async Task Destruct_leaves_deck_untouched_when_owned_still_covers()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 5, dustReward: 50);
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 1);
        await factory.AddCardToDeckAsync(viewerId, Format.Rotation, 1, cardId: 10001001L, count: 2);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        // Destruct 2 — owned drops from 5 to 3 — deck still uses only 2, no strip needed.
        var outcome = await repo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 2 } });
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var deck = await db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Decks)
            .Include(d => d.Cards).ThenInclude(c => c.Card)
            .FirstAsync(d => d.Format == Format.Rotation && d.Number == 1);

        Assert.That(deck.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(2),
            "deck untouched because owned (3) still covers usage (2)");
    }

    [Test]
    public async Task Create_single_card_debits_vials_and_grants_copy()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.True, outcome.Error?.ToString());
        Assert.That(outcome.Result!.NewRedEtherTotal, Is.EqualTo(800UL), "1000 - 200 = 800");

        var grants = outcome.Result!.Grants;
        Assert.That(grants.Any(g => g.RewardType == UserGoodsType.Card
                                 && g.RewardId   == 10001001L
                                 && g.RewardNum  == 1), Is.True);

        // Verify persisted state
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(800UL));
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Create_batch_charges_sum_and_grants_each_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001002L, count: 0, craftCost: 800);
        await factory.SetRedEtherAsync(viewerId, 5_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int>
        {
            { 10001001L, 2 },
            { 10001002L, 1 },
        });

        Assert.That(outcome.IsSuccess, Is.True);
        // 2 * 200 + 1 * 800 = 1200 → 5000 - 1200 = 3800
        Assert.That(outcome.Result!.NewRedEtherTotal, Is.EqualTo(3_800UL));

        var grants = outcome.Result!.Grants;
        Assert.That(grants.Any(g => g.RewardId == 10001001L && g.RewardNum == 2), Is.True);
        Assert.That(grants.Any(g => g.RewardId == 10001002L && g.RewardNum == 1), Is.True);
    }

    [Test]
    public async Task Create_rejects_unknown_card_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { 99_999_999L, 1 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(CreateError.UnknownCard));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(1_000UL), "no debit on rejection");
    }

    [Test]
    public async Task Create_rejects_not_craftable_card_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // craftCost=0 mirrors IsNotCraftDestruct on basic/token cards
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 0, dustReward: 0);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(CreateError.NotCraftable));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(1_000UL));
    }

    [Test]
    public async Task Create_rejects_when_would_exceed_max_copies()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Viewer already owns 2 copies — crafting 2 more would push to 4, exceeding MaxCopies=3.
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { 10001001L, 2 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(CreateError.WouldExceedMaxCopies));
    }

    [Test]
    public async Task Create_at_boundary_2_to_3_succeeds()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        // 2 + 1 = 3 = MaxCopies — must succeed
        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Create_rejects_insufficient_vials_without_mutation()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 199UL);   // one less than needed

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(CreateError.InsufficientVials));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(199UL), "no debit on rejection");
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(0), "no grant on rejection");
    }

    [Test]
    public async Task Create_validates_full_batch_before_mutating()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 0, craftCost: 200);
        // Second card has count=3 already — adding any would exceed MaxCopies
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001002L, count: 3, craftCost: 200);
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int>
        {
            { 10001001L, 1 },   // would-be valid
            { 10001002L, 1 },   // would push to 4 — fails validation
        });

        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(CreateError.WouldExceedMaxCopies));

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Currency.RedEther, Is.EqualTo(1_000UL), "no debit when batch fails");
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).Count, Is.EqualTo(0), "valid card untouched");
    }

    [Test]
    public async Task Create_first_time_owner_triggers_cosmetic_cascade()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Set up a card with a cosmetic-cascade row pointing at a Skin the viewer doesn't own.
        // Use ids outside the seeded 10001001–10001003 range so the cascade can't accidentally
        // pick up unrelated rows.
        const long cardId = 999_003_010L;
        const long skinId = 999_003_011L;
        ctx.Cards.Add(new ShadowverseCardEntry
        {
            Id = cardId, Name = "CreateCascadeCard", Rarity = Rarity.Gold,
            CollectionInfo = new CardCollectionInfo { CraftCost = 800, DustReward = 200 },
        });
        ctx.LeaderSkins.Add(new LeaderSkinEntry { Id = (int)skinId, Name = "CreateCascadeSkin" });
        ctx.CardCosmeticRewards.Add(new CardCosmeticReward
        {
            CardId = cardId, Type = CosmeticType.Skin, CosmeticId = skinId, Quantity = 1,
        });
        await ctx.SaveChangesAsync();

        // Give the viewer enough RedEther in a separate scope so the helper's reset doesn't fire.
        await factory.SetRedEtherAsync(viewerId, 1_000UL);

        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();
        var outcome = await repo.CreateCards(viewerId, new Dictionary<long, int> { { cardId, 1 } });

        Assert.That(outcome.IsSuccess, Is.True);

        var grants = outcome.Result!.Grants;
        // One Card grant + one Skin cascade grant
        Assert.That(grants.Any(g => g.RewardType == UserGoodsType.Card && g.RewardId == cardId), Is.True);
        Assert.That(grants.Any(g => g.RewardType == UserGoodsType.Skin && g.RewardId == skinId), Is.True);

        var viewer = await ctx.Viewers
            .Include(v => v.LeaderSkins)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.LeaderSkins.Any(s => s.Id == (int)skinId), Is.True, "cascade actually granted the skin");
    }

    [Test]
    public async Task SetProtected_flips_flag_on_owned_card()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, isProtected: false);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.SetProtected(viewerId, 10001001L, isProtected: true);
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).IsProtected, Is.True);
    }

    [Test]
    public async Task SetProtected_unsets_flag_when_isProtected_false()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 2, isProtected: true);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.SetProtected(viewerId, 10001001L, isProtected: false);
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Cards.First(c => c.Card.Id == 10001001L).IsProtected, Is.False);
    }

    [Test]
    public async Task SetProtected_allows_zero_count_row()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Round-trip: own 1 → destruct 1 → Count=0 row remains → protect succeeds
        await factory.SeedOwnedCardAsync(viewerId, cardId: 10001001L, count: 1, dustReward: 50);
        using var setup = factory.Services.CreateScope();
        var setupRepo = setup.ServiceProvider.GetRequiredService<ICardInventoryRepository>();
        var destruct = await setupRepo.DestructCards(viewerId, new Dictionary<long, int> { { 10001001L, 1 } });
        Assert.That(destruct.IsSuccess, Is.True, "setup precondition: destruct-to-zero");

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();
        var outcome = await repo.SetProtected(viewerId, 10001001L, isProtected: true);
        Assert.That(outcome.IsSuccess, Is.True);

        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        var owned = viewer.Cards.First(c => c.Card.Id == 10001001L);
        Assert.That(owned.Count, Is.EqualTo(0));
        Assert.That(owned.IsProtected, Is.True, "protect on zero-count row must persist");
    }

    [Test]
    public async Task SetProtected_unknown_card_returns_error()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // No OwnedCardEntry row at all

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICardInventoryRepository>();

        var outcome = await repo.SetProtected(viewerId, 99_999_999L, isProtected: true);
        Assert.That(outcome.IsSuccess, Is.False);
        Assert.That(outcome.Error, Is.EqualTo(ProtectError.UnknownCard));
    }
}
