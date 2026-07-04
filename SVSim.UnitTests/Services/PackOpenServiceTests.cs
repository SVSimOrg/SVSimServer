using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.PackDrawTables;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class PackOpenServiceTests
{
    private sealed class ScriptedRandom : IRandom
    {
        private readonly double[] _seq; private int _i;
        public ScriptedRandom(params double[] seq) { _seq = seq; }
        public double NextDouble() => _seq[_i++ % _seq.Length];
        public int Next(int maxExclusive) => (int)(NextDouble() * maxExclusive);
    }

    private sealed class NoFoil : ICardFoilLookup
    {
        public ShadowverseCardEntry? TryGetFoilTwin(long baseCardId) => null;
    }

    private static PackConfigEntry StandardPack(int id = 10000) => new()
    {
        Id = id, BasePackId = id, PackCategory = PackCategory.None,
    };

    private static PackDrawTable AllBronzeTable() => new()
    {
        Config = new PackDrawConfigEntry { Id = 10000, AnimationRatePct = 0 },
        SlotRates = new[]
        {
            new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Bronze, RatePct = 100.0 },
            new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, RatePct = 100.0 },
        },
        CardWeights = new[]
        {
            new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Bronze, CardId = 1, RatePct = 70 },
            new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Bronze, CardId = 2, RatePct = 30 },
            new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, CardId = 1, RatePct = 100 },
        },
    };

    [Test]
    public void Draw_returns_eight_cards_for_one_pack()
    {
        var svc = new PackOpenService();
        var rng = new ScriptedRandom(0.1);

        var result = svc.Draw(AllBronzeTable(), StandardPack(), 1,
            excludeCardIds: Array.Empty<long>(), ownedCardIds: Array.Empty<long>(),
            new NoFoil(), rng);

        Assert.That(result.Cards.Count, Is.EqualTo(8));
        Assert.That(result.Cards.All(c => c.CardId == 1), Is.True);
    }

    [Test]
    public void Draw_picks_card_by_per_card_weight_within_tier()
    {
        var svc = new PackOpenService();
        // Tier roll always lands in Bronze (only tier). Card pick rng=0.8 -> within Bronze
        // band > 0.7 -> card 2. Slot 8 has only card 1 in its pool so it always picks card 1.
        var rng = new ScriptedRandom(0.0, 0.8);

        var result = svc.Draw(AllBronzeTable(), StandardPack(), 1,
            Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rng);

        Assert.That(result.Cards.Take(7).All(c => c.CardId == 2), Is.True, "slots 1-7 should pick card 2");
        Assert.That(result.Cards[7].CardId, Is.EqualTo(1), "slot 8 pool only contains card 1");
    }

    [Test]
    public void Draw_rate_less_branch_picks_only_unowned()
    {
        var pack = new PackConfigEntry { Id = 98001, BasePackId = 98001, PackCategory = PackCategory.SpecialCardPack };
        var table = new PackDrawTable
        {
            Config = new PackDrawConfigEntry { Id = 98001, AnimationRatePct = 0, HasBonusSlot = true, SpecialKind = "leader_card" },
            SlotRates = new[]
            {
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.General, Tier = DrawTier.Bronze,  RatePct = 100.0 },
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze,  RatePct = 100.0 },
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, RatePct = 100.0 },
            },
            CardWeights = new[]
            {
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.General, Tier = DrawTier.Bronze,  CardId = 10, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze,  CardId = 10, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, CardId = 300, RatePct = null, IsLeader = true },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, CardId = 301, RatePct = null, IsLeader = true },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, CardId = 302, RatePct = null, IsLeader = true },
            },
        };
        var svc = new PackOpenService();
        var rng = new ScriptedRandom(0.1);

        var result = svc.Draw(table, pack, packNumber: 10,
            excludeCardIds: Array.Empty<long>(),
            ownedCardIds: new long[] { 300, 301 },
            new NoFoil(), rng);

        Assert.That(result.Cards.Count, Is.EqualTo(81));   // 10 packs * 8 + 1 bonus
        var bonus = result.Cards[^1];
        Assert.That(bonus.CardId, Is.EqualTo(302));
    }

    [Test]
    public void Draw_rate_less_falls_back_to_full_pool_when_all_owned()
    {
        var pack = new PackConfigEntry { Id = 98001, BasePackId = 98001 };
        var table = new PackDrawTable
        {
            Config = new PackDrawConfigEntry { Id = 98001, AnimationRatePct = 0, HasBonusSlot = true },
            SlotRates = new[]
            {
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.General, Tier = DrawTier.Bronze,  RatePct = 100.0 },
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze,  RatePct = 100.0 },
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, RatePct = 100.0 },
            },
            CardWeights = new[]
            {
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.General, Tier = DrawTier.Bronze,  CardId = 10, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze,  CardId = 10, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, CardId = 300, RatePct = null, IsLeader = true },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, CardId = 301, RatePct = null, IsLeader = true },
            },
        };
        var svc = new PackOpenService();
        var rng = new ScriptedRandom(0.1);

        var result = svc.Draw(table, pack, packNumber: 10,
            excludeCardIds: Array.Empty<long>(),
            ownedCardIds: new long[] { 300, 301 },
            new NoFoil(), rng);

        var bonus = result.Cards[^1];
        Assert.That(bonus.CardId, Is.AnyOf(300L, 301L));
    }

    [Test]
    [Category("Slow")]
    public void Draw_observed_tier_rates_track_seed_within_half_a_percent()
    {
        // Synthetic Classic pack: Bronze=76.5/Silver=16/Gold=6/Legendary=1.5 in general slots;
        // slot 8 is Silver=92.5/Gold=6/Legendary=1.5 (no Bronze).
        var table = new PackDrawTable
        {
            Config = new PackDrawConfigEntry { Id = 10000, AnimationRatePct = 0 },
            SlotRates = new[]
            {
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Bronze,    RatePct = 76.5 },
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Silver,    RatePct = 16.0 },
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Gold,      RatePct = 6.0  },
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Legendary, RatePct = 1.5  },
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Silver,    RatePct = 92.5 },
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Gold,      RatePct = 6.0  },
                new PackDrawSlotRateEntry { PackId = 10000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Legendary, RatePct = 1.5  },
            },
            CardWeights = new[]
            {
                new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Bronze,    CardId = 1, RatePct = 76.5 },
                new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Silver,    CardId = 2, RatePct = 16.0 },
                new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Gold,      CardId = 3, RatePct = 6.0 },
                new PackDrawCardWeightEntry { PackId = 10000, Slot = DrawSlot.General, Tier = DrawTier.Legendary, CardId = 4, RatePct = 1.5 },
            },
        };
        var svc = new PackOpenService();
        var rng = new SystemRandom(42);
        var pack = new PackConfigEntry { Id = 10000 };
        int totalSlots = 200_000;
        int bronze = 0, silver = 0, gold = 0, legendary = 0;

        // 25_000 packs * 7 general slots = 175_000 general-slot observations.
        for (int i = 0; i < totalSlots / 8; i++)
        {
            var r = svc.Draw(table, pack, 1, Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rng);
            for (int s = 0; s < 7; s++)
            {
                switch (r.Cards[s].Rarity)
                {
                    case Rarity.Bronze:    bronze++; break;
                    case Rarity.Silver:    silver++; break;
                    case Rarity.Gold:      gold++; break;
                    case Rarity.Legendary: legendary++; break;
                }
            }
        }

        double n = bronze + silver + gold + legendary;
        Assert.That(100 * bronze    / n, Is.EqualTo(76.5).Within(0.5));
        Assert.That(100 * silver    / n, Is.EqualTo(16.0).Within(0.5));
        Assert.That(100 * gold      / n, Is.EqualTo(6.0).Within(0.5));
        Assert.That(100 * legendary / n, Is.EqualTo(1.5).Within(0.5));
    }

    [Test]
    public void Draw_does_not_emit_bonus_for_packNumber_less_than_10()
    {
        var pack = new PackConfigEntry { Id = 98001 };
        var table = new PackDrawTable
        {
            Config = new PackDrawConfigEntry { Id = 98001, HasBonusSlot = true, AnimationRatePct = 0 },
            SlotRates = new[]
            {
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.General, Tier = DrawTier.Bronze,  RatePct = 100 },
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze,  RatePct = 100 },
                new PackDrawSlotRateEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, RatePct = 100 },
            },
            CardWeights = new[]
            {
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.General, Tier = DrawTier.Bronze,  CardId = 1, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze,  CardId = 1, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 98001, Slot = DrawSlot.Bonus,   Tier = DrawTier.Special, CardId = 999, RatePct = null, IsLeader = true },
            },
        };
        var svc = new PackOpenService();
        var rng = new ScriptedRandom(0.1);

        var result = svc.Draw(table, pack, packNumber: 1,
            Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rng);

        Assert.That(result.Cards.Count, Is.EqualTo(8));
    }

    // ── Rotation-starter (class-axis) draws ────────────────────────────────
    //
    // RotationStarter packs partition the card pool by class_id (1..8). Slot
    // rates are class-invariant (verified against shadowverse.com/drawrates/
    // klan 1..8 captures, all identical). Each class's card pool is a distinct
    // subset; cards shared across classes are duplicated in the seed data with
    // separate (class_id, card_id) rows. The Draw() method's classId parameter
    // filters CardWeights to rows where ClassId == classId OR ClassId is null.

    private static PackDrawTable RotationStarterTable() => new()
    {
        Config = new PackDrawConfigEntry { Id = 93025, AnimationRatePct = 0 },
        SlotRates = new[]
        {
            // Same rates regardless of class — no ClassId on slot rates.
            new PackDrawSlotRateEntry { PackId = 93025, Slot = DrawSlot.General, Tier = DrawTier.Bronze, RatePct = 100.0 },
            new PackDrawSlotRateEntry { PackId = 93025, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, RatePct = 100.0 },
        },
        CardWeights = new[]
        {
            // class 1 sees cards 11 and 12; class 2 sees cards 21 and 22; class 3 sees only card 33.
            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 1, CardId = 11, RatePct = 50 },
            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 1, CardId = 12, RatePct = 50 },
            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, ClassId = 1, CardId = 11, RatePct = 100 },

            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 2, CardId = 21, RatePct = 50 },
            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 2, CardId = 22, RatePct = 50 },
            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, ClassId = 2, CardId = 21, RatePct = 100 },

            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 3, CardId = 33, RatePct = 100 },
            new PackDrawCardWeightEntry { PackId = 93025, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, ClassId = 3, CardId = 33, RatePct = 100 },
        },
    };

    [Test]
    public void Draw_with_class_id_picks_only_that_class_pool()
    {
        var svc = new PackOpenService();
        var rng = new ScriptedRandom(0.0);  // always pick first option

        var rsPack = new PackConfigEntry { Id = 93025, BasePackId = 93025, PackCategory = PackCategory.RotationStarterCardPack };
        var class3Result = svc.Draw(RotationStarterTable(), rsPack, 1,
            Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rng, classId: 3);

        Assert.That(class3Result.Cards.Count, Is.EqualTo(8));
        Assert.That(class3Result.Cards.All(c => c.CardId == 33), Is.True,
            "class 3's pool only contains card 33; every slot must pick it");
    }

    [Test]
    public void Draw_with_different_class_ids_picks_disjoint_pools()
    {
        var svc = new PackOpenService();
        var rsPack = new PackConfigEntry { Id = 93025, BasePackId = 93025, PackCategory = PackCategory.RotationStarterCardPack };

        var rngClass1 = new ScriptedRandom(0.0);
        var class1 = svc.Draw(RotationStarterTable(), rsPack, 1,
            Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rngClass1, classId: 1);
        var rngClass2 = new ScriptedRandom(0.0);
        var class2 = svc.Draw(RotationStarterTable(), rsPack, 1,
            Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rngClass2, classId: 2);

        // Class 1 cards are 11/12, class 2 cards are 21/22 — verify no cross-contamination.
        Assert.That(class1.Cards.All(c => c.CardId == 11 || c.CardId == 12), Is.True,
            $"class 1 should only see cards 11/12 but got {string.Join(",", class1.Cards.Select(c => c.CardId))}");
        Assert.That(class2.Cards.All(c => c.CardId == 21 || c.CardId == 22), Is.True,
            $"class 2 should only see cards 21/22 but got {string.Join(",", class2.Cards.Select(c => c.CardId))}");
    }

    [Test]
    public void Draw_without_class_id_skips_class_tagged_rows()
    {
        // Defensive: if a non-RS draw call lands on a table that ALSO has class-tagged rows,
        // the class-tagged rows must not leak in. Mixed-table case shouldn't occur in practice
        // but exercises the filter contract.
        var svc = new PackOpenService();
        var rng = new ScriptedRandom(0.0);

        var mixed = new PackDrawTable
        {
            Config = new PackDrawConfigEntry { Id = 50000, AnimationRatePct = 0 },
            SlotRates = new[]
            {
                new PackDrawSlotRateEntry { PackId = 50000, Slot = DrawSlot.General, Tier = DrawTier.Bronze, RatePct = 100.0 },
                new PackDrawSlotRateEntry { PackId = 50000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, RatePct = 100.0 },
            },
            CardWeights = new[]
            {
                new PackDrawCardWeightEntry { PackId = 50000, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = null, CardId = 99, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 50000, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, ClassId = null, CardId = 99, RatePct = 100 },
                new PackDrawCardWeightEntry { PackId = 50000, Slot = DrawSlot.General, Tier = DrawTier.Bronze, ClassId = 1,    CardId = 11, RatePct = 100 },
            },
        };
        var result = svc.Draw(mixed, new PackConfigEntry { Id = 50000, BasePackId = 50000, PackCategory = PackCategory.None },
            1, Array.Empty<long>(), Array.Empty<long>(), new NoFoil(), rng);
        Assert.That(result.Cards.All(c => c.CardId == 99), Is.True,
            $"non-class draw should ignore class-tagged rows; got {string.Join(",", result.Cards.Select(c => c.CardId))}");
    }
}
