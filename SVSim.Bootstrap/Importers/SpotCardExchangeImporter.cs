using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of the spot card exchange catalog from <c>seeds/spot-card-exchange.json</c>.
/// Source is the wire <c>/spot_card_exchange/top</c> response, extracted via
/// <c>data_dumps/scripts/extract-spot-card-exchange.py</c>. Rows missing from the seed are
/// LEFT INTACT.
/// </summary>
public class SpotCardExchangeImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        string path = Path.Combine(seedDir, "spot-card-exchange.json");
        var seed = SeedLoader.LoadList<SpotCardExchangeSeed>(path);
        if (seed.Count == 0)
        {
            Console.WriteLine("[SpotCardExchangeImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.SpotCardExchangeCatalog.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.CardId == 0) continue;

            var entry = existing.TryGetValue(s.CardId, out var ex)
                ? ex : new SpotCardExchangeEntry { Id = s.CardId };

            entry.ClassId = s.ClassId;
            entry.ExchangePoint = s.ExchangePoint;
            entry.TsRotationId = s.TsRotationId;
            entry.IsPreRelease = s.IsPreRelease;
            entry.IsEnabled = true;

            if (ex is null)
            {
                context.SpotCardExchangeCatalog.Add(entry);
                existing[s.CardId] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[SpotCardExchangeImporter] +{created}/~{updated}");
        return created + updated;
    }
}
