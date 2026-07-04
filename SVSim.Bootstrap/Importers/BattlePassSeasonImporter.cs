using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of battle-pass seasons from <c>seeds/battle-pass-seasons.json</c>.
/// Rows missing from the seed are LEFT INTACT (historic seasons).
/// </summary>
public class BattlePassSeasonImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<BattlePassSeasonSeed>(Path.Combine(seedDir, "battle-pass-seasons.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[BattlePassSeasonImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.BattlePassSeasons.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;
        foreach (var s in seed)
        {
            if (s.Id == 0) continue;
            var entry = existing.TryGetValue(s.Id, out var ex) ? ex : new BattlePassSeasonEntry { Id = s.Id };
            entry.Name = s.Name;
            entry.MaxLevel = s.MaxLevel;
            // Postgres 'timestamp with time zone' only accepts UTC offset; JST-offset values
            // from the seed are converted to UTC to preserve the instant. Comparisons via
            // DateTimeOffset are instant-based, so the JST→UTC conversion is semantically lossless.
            entry.StartDate = DateTimeOffset.Parse(s.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            entry.EndDate = DateTimeOffset.Parse(s.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            entry.CanPurchase = s.CanPurchase;
            entry.PriceCrystal = s.PriceCrystal;
            entry.Description = s.Description;
            if (ex is null) { context.BattlePassSeasons.Add(entry); existing[s.Id] = entry; created++; }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[BattlePassSeasonImporter] +{created}/~{updated}");
        return created + updated;
    }
}
