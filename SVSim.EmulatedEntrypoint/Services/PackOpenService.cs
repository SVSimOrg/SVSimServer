using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.PackDrawTables;
using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Draws cards from a pack's per-pack draw table. Slot tier and per-card weights are sampled
/// directly from the seeded data. The bonus slot fires once at the end of a 10-pack open
/// when <see cref="PackDrawConfigEntry.HasBonusSlot"/> is set.
/// </summary>
public class PackOpenService
{
    private const int CardsPerPack = 8;

    public DrawResult Draw(
        PackDrawTable drawTable,
        PackConfigEntry pack,
        int packNumber,
        IReadOnlyCollection<long> excludeCardIds,
        IReadOnlyCollection<long> ownedCardIds,
        ICardFoilLookup foilLookup,
        IRandom rng,
        int? classId = null)
    {
        // Filter the card pool by class: rotation-starter packs partition their
        // card pool per class (1..8); non-RS packs leave ClassId null and apply
        // to any draw. A row matches if it carries no class restriction OR if
        // its class matches the chosen class. When classId is null (non-RS
        // draw), only class-less rows are kept — this prevents class-tagged
        // rows from leaking into a non-RS draw if the data is inconsistent.
        var pool = classId.HasValue
            ? drawTable.CardWeights.Where(w => w.ClassId == null || w.ClassId == classId.Value)
            : drawTable.CardWeights.Where(w => w.ClassId == null);

        var byKey = pool
            .GroupBy(w => (w.Slot, w.Tier))
            .ToDictionary(g => g.Key, g => g.ToList());

        var slotRatesByKey = drawTable.SlotRates
            .GroupBy(s => s.Slot)
            .ToDictionary(g => g.Key, g => g.ToList());

        var slots = new List<DrawnCard>(packNumber * CardsPerPack + 1);

        for (int p = 0; p < packNumber; p++)
        {
            for (int s = 0; s < CardsPerPack; s++)
            {
                int slotNum = s + 1;
                var slot = slotNum == CardsPerPack ? DrawSlot.Eighth : DrawSlot.General;
                var drawn = DrawOne(slot, drawTable, byKey, slotRatesByKey,
                    excludeCardIds, ownedCardIds, foilLookup, rng);
                slots.Add(drawn);
            }
        }

        if (drawTable.Config.HasBonusSlot && packNumber == 10)
        {
            var bonus = DrawOne(DrawSlot.Bonus, drawTable, byKey, slotRatesByKey,
                excludeCardIds, ownedCardIds, foilLookup, rng);
            slots.Add(bonus);
        }

        return new DrawResult(slots);
    }

    private static DrawnCard DrawOne(
        DrawSlot slot,
        PackDrawTable drawTable,
        Dictionary<(DrawSlot, DrawTier), List<PackDrawCardWeightEntry>> byKey,
        Dictionary<DrawSlot, List<PackDrawSlotRateEntry>> slotRatesByKey,
        IReadOnlyCollection<long> excludeCardIds,
        IReadOnlyCollection<long> ownedCardIds,
        ICardFoilLookup foilLookup,
        IRandom rng)
    {
        var slotRates = slotRatesByKey.TryGetValue(slot, out var sr) ? sr : new();
        if (slotRates.Count == 0)
            throw new InvalidOperationException(
                $"PackOpenService: no slot rates for pack {drawTable.Config.Id} slot {slot}");

        var tiers = slotRates.Select(r => r.Tier).ToList();
        var tierWeights = slotRates.Select(r => r.RatePct).ToList();
        var tier = WeightedPick.Pick(rng, tiers, tierWeights);

        // For slot 8 (and bonus), drawrates pages often quote per-rarity slot rates but no per-card
        // breakdown — the card pool is the same as the general slot's per-tier pool. Fall back to
        // (General, tier) when (slot, tier) has no card weights.
        if (!byKey.TryGetValue((slot, tier), out var rows) && slot != DrawSlot.General)
        {
            byKey.TryGetValue((DrawSlot.General, tier), out rows);
        }
        var pool = rows ?? new();
        var filtered = pool.Where(w => !excludeCardIds.Contains(w.CardId)).ToList();

        if (filtered.Count == 0)
            return FallbackAcrossTiers(slot, byKey, excludeCardIds, foilLookup, rng, drawTable);

        bool rateLess = filtered.All(w => w.RatePct == null);

        PackDrawCardWeightEntry picked;
        if (rateLess)
        {
            var unowned = filtered.Where(w => !ownedCardIds.Contains(w.CardId)).ToList();
            var sourcePool = unowned.Count > 0 ? unowned : filtered;
            picked = sourcePool[rng.Next(sourcePool.Count)];
        }
        else
        {
            var metered = filtered.Where(w => w.RatePct.HasValue).ToList();
            if (metered.Count == 0)
                return FallbackAcrossTiers(slot, byKey, excludeCardIds, foilLookup, rng, drawTable);
            picked = WeightedPick.Pick(rng, metered, metered.Select(w => w.RatePct!.Value).ToList());
        }

        long cardId = picked.CardId;

        if (drawTable.Config.AnimationRatePct > 0
            && rng.NextDouble() < drawTable.Config.AnimationRatePct / 100.0)
        {
            var foil = foilLookup.TryGetFoilTwin(cardId);
            if (foil is not null) cardId = foil.Id;
        }

        var rarity = TierToRarity(picked);
        return new DrawnCard(cardId, rarity);
    }

    private static DrawnCard FallbackAcrossTiers(
        DrawSlot slot,
        Dictionary<(DrawSlot, DrawTier), List<PackDrawCardWeightEntry>> byKey,
        IReadOnlyCollection<long> excludeCardIds,
        ICardFoilLookup foilLookup,
        IRandom rng,
        PackDrawTable drawTable)
    {
        foreach (var tier in new[] { DrawTier.Legendary, DrawTier.Gold, DrawTier.Silver, DrawTier.Bronze, DrawTier.Special })
        {
            if (!byKey.TryGetValue((slot, tier), out var rows)) continue;
            var filtered = rows.Where(w => !excludeCardIds.Contains(w.CardId)).ToList();
            if (filtered.Count == 0) continue;
            var picked = filtered[rng.Next(filtered.Count)];
            return new DrawnCard(picked.CardId, TierToRarity(picked));
        }
        throw new InvalidOperationException(
            $"PackOpenService: pool empty after exclude filter for pack {drawTable.Config.Id} slot {slot}.");
    }

    private static Rarity TierToRarity(PackDrawCardWeightEntry w) => w.Tier switch
    {
        DrawTier.Bronze    => Rarity.Bronze,
        DrawTier.Silver    => Rarity.Silver,
        DrawTier.Gold      => Rarity.Gold,
        DrawTier.Legendary => Rarity.Legendary,
        // Special tier cards typically have intrinsic Rarity.Legendary; the wire response
        // surfaces Rarity as an int for client coloring and the card_id is the source of
        // truth for what's granted.
        DrawTier.Special   => Rarity.Legendary,
        _ => Rarity.Bronze,
    };
}
