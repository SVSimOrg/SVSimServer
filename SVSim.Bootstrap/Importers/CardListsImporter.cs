using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the six card-id-keyed tables from load-index seeds:
/// SpotCards, ReprintedCards, UnlimitedRestrictions, LoadingExclusionCards,
/// MaintenanceCards, FeatureMaintenances. Loads the Cards FK set once for orphan warnings.
/// Rows missing from a seed are LEFT INTACT — a partial seed shouldn't silently delete entries.
/// FeatureMaintenances clears-and-rewrites because its synthetic ordinal Id has no natural-key semantics.
/// </summary>
public class CardListsImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var knownCards = new HashSet<long>(await context.Cards.Select(c => c.Id).ToListAsync());
        int total = 0;
        total += await ImportSpot(context, seedDir, knownCards);
        total += await ImportReprinted(context, seedDir, knownCards);
        total += await ImportUnlimited(context, seedDir, knownCards);
        total += await ImportLoadingExclusion(context, seedDir, knownCards);
        total += await ImportMaintenance(context, seedDir);
        total += await ImportFeatureMaintenances(context, seedDir);
        await context.SaveChangesAsync();
        return total;
    }

    private async Task<int> ImportSpot(SVSimDbContext context, string seedDir, HashSet<long> knownCards)
    {
        var seed = SeedLoader.LoadList<SpotCardSeed>(Path.Combine(seedDir, "spot-cards.json"));
        if (seed.Count == 0) return 0;
        var existing = await context.SpotCards.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0, orphans = 0;
        foreach (var s in seed)
        {
            if (s.CardId == 0) continue;
            if (!knownCards.Contains(s.CardId)) orphans++;
            var entry = existing.TryGetValue(s.CardId, out var ex) ? ex : new SpotCardEntry { Id = s.CardId };
            entry.Cost = s.Cost;
            if (ex is null) { context.SpotCards.Add(entry); existing[s.CardId] = entry; created++; }
            else updated++;
        }
        WarnOrphans("SpotCards", orphans);
        Console.WriteLine($"[CardListsImporter] SpotCards +{created}/~{updated}");
        return created + updated;
    }

    private async Task<int> ImportReprinted(SVSimDbContext context, string seedDir, HashSet<long> knownCards)
    {
        var seed = SeedLoader.LoadList<ReprintedCardSeed>(Path.Combine(seedDir, "reprinted-cards.json"));
        if (seed.Count == 0) return 0;
        var existing = await context.ReprintedCards.ToDictionaryAsync(e => e.Id);
        int created = 0, orphans = 0;
        foreach (var s in seed)
        {
            if (s.CardId == 0) continue;
            if (!knownCards.Contains(s.CardId)) orphans++;
            if (existing.ContainsKey(s.CardId)) continue;
            var entry = new ReprintedCardEntry { Id = s.CardId };
            context.ReprintedCards.Add(entry);
            existing[s.CardId] = entry;
            created++;
        }
        WarnOrphans("ReprintedCards", orphans);
        Console.WriteLine($"[CardListsImporter] ReprintedCards +{created}");
        return created;
    }

    private async Task<int> ImportUnlimited(SVSimDbContext context, string seedDir, HashSet<long> knownCards)
    {
        var seed = SeedLoader.LoadList<UnlimitedRestrictionSeed>(Path.Combine(seedDir, "unlimited-restrictions.json"));
        if (seed.Count == 0) return 0;
        var existing = await context.UnlimitedRestrictions.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0, orphans = 0;
        foreach (var s in seed)
        {
            if (s.CardId == 0) continue;
            if (!knownCards.Contains(s.CardId)) orphans++;
            var entry = existing.TryGetValue(s.CardId, out var ex) ? ex : new UnlimitedRestrictionEntry { Id = s.CardId };
            entry.RestrictionValue = s.RestrictionValue;
            if (ex is null) { context.UnlimitedRestrictions.Add(entry); existing[s.CardId] = entry; created++; }
            else updated++;
        }
        WarnOrphans("UnlimitedRestrictions", orphans);
        Console.WriteLine($"[CardListsImporter] UnlimitedRestrictions +{created}/~{updated}");
        return created + updated;
    }

    private async Task<int> ImportLoadingExclusion(SVSimDbContext context, string seedDir, HashSet<long> knownCards)
    {
        var seed = SeedLoader.LoadList<LoadingExclusionCardSeed>(Path.Combine(seedDir, "loading-exclusion-cards.json"));
        if (seed.Count == 0) return 0;
        var existing = await context.LoadingExclusionCards.ToDictionaryAsync(e => e.Id);
        int created = 0, orphans = 0;
        foreach (var s in seed)
        {
            if (s.CardId == 0) continue;
            if (!knownCards.Contains(s.CardId)) orphans++;
            if (existing.ContainsKey(s.CardId)) continue;
            var entry = new LoadingExclusionCardEntry { Id = s.CardId };
            context.LoadingExclusionCards.Add(entry);
            existing[s.CardId] = entry;
            created++;
        }
        WarnOrphans("LoadingExclusionCards", orphans);
        Console.WriteLine($"[CardListsImporter] LoadingExclusionCards +{created}");
        return created;
    }

    private async Task<int> ImportMaintenance(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<MaintenanceCardSeed>(Path.Combine(seedDir, "maintenance-cards.json"));
        if (seed.Count == 0) return 0;
        var existing = await context.MaintenanceCards.ToDictionaryAsync(e => e.Id);
        int created = 0;
        foreach (var s in seed)
        {
            if (s.CardId == 0) continue;
            if (existing.ContainsKey(s.CardId)) continue;
            var entry = new MaintenanceCardEntry { Id = s.CardId };
            context.MaintenanceCards.Add(entry);
            existing[s.CardId] = entry;
            created++;
        }
        Console.WriteLine($"[CardListsImporter] MaintenanceCards +{created}");
        return created;
    }

    private async Task<int> ImportFeatureMaintenances(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<FeatureMaintenanceSeed>(Path.Combine(seedDir, "feature-maintenances.json"));
        if (seed.Count == 0) return 0;
        // FeatureMaintenances has a synthetic int Id assigned by the extractor (1-based ordinal).
        // FeatureMaintenances use a synthetic ordinal id from the extractor; we clear-and-rewrite to
        // keep re-runs idempotent and match "the latest seed is authoritative". Pre-existing rows
        // with seed-absent ids are dropped here (acceptable: only synthetic ordinals, no FKs
        // reference this table).
        var existing = await context.FeatureMaintenances.ToListAsync();
        context.FeatureMaintenances.RemoveRange(existing);
        int created = 0;
        foreach (var s in seed)
        {
            if (s.Id == 0) continue;
            context.FeatureMaintenances.Add(new FeatureMaintenanceEntry
            {
                Id = s.Id,
                FeatureKey = s.FeatureKey,
                Data = s.Data.ValueKind == JsonValueKind.Undefined ? "{}" : JsonSerializer.Serialize(s.Data),
            });
            created++;
        }
        Console.WriteLine($"[CardListsImporter] FeatureMaintenances: -{existing.Count}/+{created}");
        return created;
    }

    private static void WarnOrphans(string label, int count)
    {
        if (count > 0)
            Console.Error.WriteLine($"[CardListsImporter] Warning: {label} has {count} orphan card_id(s) — run CardImporter first for clean references.");
    }
}
