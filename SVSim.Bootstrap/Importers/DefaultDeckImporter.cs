using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of default decks from <c>seeds/default-decks.json</c>. Warns on orphan card
/// references (card_id not in Cards table) but never fails — CardImporter must run first for a
/// clean warning-free run. Rows missing from the seed are LEFT INTACT.
/// </summary>
public class DefaultDeckImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<DefaultDeckSeed>(Path.Combine(seedDir, "default-decks.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[DefaultDeckImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.DefaultDecks.ToDictionaryAsync(e => e.Id);
        var knownCards = new HashSet<long>(await context.Cards.Select(c => c.Id).ToListAsync());
        int created = 0, updated = 0, orphans = 0;

        foreach (var s in seed)
        {
            if (s.Id == 0) continue;
            var entry = existing.TryGetValue(s.Id, out var ex) ? ex : new DefaultDeckEntry { Id = s.Id };
            entry.ClassId = s.ClassId;
            entry.SleeveId = s.SleeveId;
            entry.LeaderSkinId = s.LeaderSkinId;
            entry.DeckName = s.DeckName;
            entry.CardIdArray = JsonSerializer.Serialize(s.CardIdArray);

            // Orphan count against card master — informational, never throws.
            foreach (var cardId in s.CardIdArray)
            {
                if (!knownCards.Contains(cardId)) orphans++;
            }

            if (ex is null) { context.DefaultDecks.Add(entry); existing[s.Id] = entry; created++; }
            else updated++;
        }

        await context.SaveChangesAsync();
        WarnOrphans("DefaultDecks.card_id_array", orphans);
        Console.WriteLine($"[DefaultDeckImporter] +{created}/~{updated}");
        return created + updated;
    }

    private static void WarnOrphans(string label, int count)
    {
        if (count > 0)
            Console.Error.WriteLine($"[DefaultDeckImporter] Warning: {label} has {count} orphan card_id(s) — run CardImporter first for clean references.");
    }
}
