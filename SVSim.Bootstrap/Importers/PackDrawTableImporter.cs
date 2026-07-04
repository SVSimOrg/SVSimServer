using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the per-pack draw table from
/// <c>seeds/pack-draw-config.json</c>, <c>pack-draw-slot-rates.json</c>, and
/// <c>pack-draw-card-weights.json</c>. Replaces wholesale per pack (config keyed on
/// pack_id; slot rates / card weights wiped and reinserted) — the upstream data is
/// post-shutdown closed, so we do not preserve hand-edits on these tables.
/// </summary>
public class PackDrawTableImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var configs = SeedLoader.LoadList<PackDrawConfigSeed>(Path.Combine(seedDir, "pack-draw-config.json"));
        var slotRates = SeedLoader.LoadList<PackDrawSlotRateSeed>(Path.Combine(seedDir, "pack-draw-slot-rates.json"));
        var cardWeights = SeedLoader.LoadList<PackDrawCardWeightSeed>(Path.Combine(seedDir, "pack-draw-card-weights.json"));

        if (configs.Count == 0)
        {
            Console.WriteLine("[PackDrawTableImporter] No seed rows; skipping.");
            return 0;
        }

        var seedPackIds = configs.Select(c => c.PackId).ToHashSet();

        // Full-replace strategy: wipe rows for any pack in the seed, then reinsert.
        await context.PackDrawCardWeights
            .Where(w => seedPackIds.Contains(w.PackId))
            .ExecuteDeleteAsync();
        await context.PackDrawSlotRates
            .Where(s => seedPackIds.Contains(s.PackId))
            .ExecuteDeleteAsync();

        var existingConfigs = await context.PackDrawConfigs
            .Where(c => seedPackIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        foreach (var s in configs)
        {
            var row = existingConfigs.TryGetValue(s.PackId, out var ex)
                ? ex : new PackDrawConfigEntry { Id = s.PackId };
            row.AnimationRatePct = s.AnimationRatePct;
            row.HasBonusSlot = s.HasBonusSlot;
            row.SpecialKind = s.SpecialKind;
            row.ShortCode = s.ShortCode;
            if (ex is null) context.PackDrawConfigs.Add(row);
        }

        foreach (var s in slotRates)
        {
            context.PackDrawSlotRates.Add(new PackDrawSlotRateEntry
            {
                PackId = s.PackId,
                Slot = ParseSlot(s.Slot),
                Tier = ParseTier(s.Tier),
                RatePct = s.RatePct,
            });
        }

        foreach (var s in cardWeights)
        {
            context.PackDrawCardWeights.Add(new PackDrawCardWeightEntry
            {
                PackId = s.PackId,
                Slot = ParseSlot(s.Slot),
                Tier = ParseTier(s.Tier),
                ClassId = s.ClassId,
                CardId = s.CardId,
                RatePct = s.RatePct,
                IsLeader = s.IsLeader,
                IsAltArt = s.IsAltArt,
            });
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PackDrawTableImporter] {configs.Count} configs / {slotRates.Count} slot rates / {cardWeights.Count} card weights");
        return configs.Count;
    }

    private static DrawSlot ParseSlot(string s) => s switch
    {
        "general" => DrawSlot.General,
        "eighth"  => DrawSlot.Eighth,
        "bonus"   => DrawSlot.Bonus,
        _ => throw new InvalidDataException($"PackDrawTableImporter: unknown slot \"{s}\""),
    };

    private static DrawTier ParseTier(string s) => s switch
    {
        "bronze"    => DrawTier.Bronze,
        "silver"    => DrawTier.Silver,
        "gold"      => DrawTier.Gold,
        "legendary" => DrawTier.Legendary,
        "special"   => DrawTier.Special,
        _ => throw new InvalidDataException($"PackDrawTableImporter: unknown tier \"{s}\""),
    };
}
