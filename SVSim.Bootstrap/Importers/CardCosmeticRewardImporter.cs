using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Reads <c>card_cosmetic_rewards.csv</c> and upserts <see cref="CardCosmeticReward"/> rows.
/// MUST run after <see cref="CardImporter"/> — the table has an FK to <c>Cards.Id</c>, so any
/// reward whose CardId isn't in the freshly-imported cards table is skipped with a warning.
/// </summary>
public class CardCosmeticRewardImporter
{
    public async Task ImportAsync(SVSimDbContext context, string dataDir)
    {
        string path = Path.Combine(dataDir, "card_cosmetic_rewards.csv");
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[CardCosmeticRewardImporter] Missing CSV: {path}");
            return;
        }

        Console.WriteLine($"[CardCosmeticRewardImporter] Reading {path}...");

        List<CardCosmeticReward> rows;
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<CardCosmeticRewardMap>();
            rows = csv.GetRecords<CardCosmeticReward>().ToList();
        }

        var validCardIds = (await context.Cards.Select(c => c.Id).ToListAsync()).ToHashSet();
        var existing = (await context.CardCosmeticRewards.ToListAsync())
            .ToDictionary(r => (r.CardId, r.Type, r.CosmeticId));

        int created = 0, updated = 0, skipped = 0;
        foreach (var r in rows)
        {
            if (!validCardIds.Contains(r.CardId))
            {
                skipped++;
                continue;
            }

            var key = (r.CardId, r.Type, r.CosmeticId);
            if (existing.TryGetValue(key, out var e))
            {
                if (e.Quantity != r.Quantity) { e.Quantity = r.Quantity; updated++; }
            }
            else
            {
                context.CardCosmeticRewards.Add(r);
                created++;
            }
        }
        await context.SaveChangesAsync();
        Console.WriteLine(
            $"[CardCosmeticRewardImporter] Done: +{created} / ~{updated}, " +
            $"skipped {skipped} (no matching card row).");
    }

    private sealed class CardCosmeticRewardMap : ClassMap<CardCosmeticReward>
    {
        public CardCosmeticRewardMap()
        {
            Map(m => m.CardId).Name("card_id");
            Map(m => m.Type).Name("type");
            Map(m => m.CosmeticId).Name("cosmetic_id");
            Map(m => m.Quantity).Name("quantity").Default(1);
            Map(m => m.Card).Ignore();
        }
    }
}
