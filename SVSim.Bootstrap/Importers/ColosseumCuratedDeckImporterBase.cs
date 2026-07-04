using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Shared upsert path for the three Colosseum curated-deck pools. Each subclass binds the
/// EF entity type + the seed filename; the load + key + diff logic lives here. Empty seed
/// files are non-fatal — the pools ship empty by default per the plan (admins fill them
/// per-event).
/// </summary>
public abstract class ColosseumCuratedDeckImporterBase<TEntity>
    where TEntity : class, IColosseumCuratedDeck, new()
{
    protected abstract string SeedFileName { get; }

    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var path = Path.Combine(seedDir, SeedFileName);
        if (!File.Exists(path))
        {
            Console.WriteLine($"[{GetType().Name}] missing {path}; skipping.");
            return 0;
        }

        var seeds = SeedLoader.LoadList<ColosseumCuratedDeckSeed>(path);
        var set = context.Set<TEntity>();
        var existing = await set.ToDictionaryAsync(d => d.DeckNo);

        int upserted = 0;
        foreach (var s in seeds)
        {
            if (existing.TryGetValue(s.DeckNo, out var row))
            {
                row.ClassId = s.ClassId;
                row.CardListJson = s.CardListJson;
                row.SleeveId = s.SleeveId;
                row.LeaderSkinId = s.LeaderSkinId;
                row.DisplayOrder = s.DisplayOrder;
            }
            else
            {
                set.Add(new TEntity
                {
                    DeckNo = s.DeckNo,
                    ClassId = s.ClassId,
                    CardListJson = s.CardListJson,
                    SleeveId = s.SleeveId,
                    LeaderSkinId = s.LeaderSkinId,
                    DisplayOrder = s.DisplayOrder,
                });
            }
            upserted++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[{GetType().Name}] upserted={upserted}");
        return upserted;
    }
}

public sealed class ColosseumCuratedDeckSeed
{
    public int DeckNo { get; set; }
    public int ClassId { get; set; }
    public string CardListJson { get; set; } = "[]";
    public long SleeveId { get; set; }
    public long LeaderSkinId { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class ColosseumHofDecksImporter : ColosseumCuratedDeckImporterBase<ColosseumHofDeck>
{
    protected override string SeedFileName => "colosseum-hof-decks.json";
}

public sealed class ColosseumWindFallDecksImporter : ColosseumCuratedDeckImporterBase<ColosseumWindFallDeck>
{
    protected override string SeedFileName => "colosseum-windfall-decks.json";
}

public sealed class ColosseumAvatarDecksImporter : ColosseumCuratedDeckImporterBase<ColosseumAvatarDeck>
{
    protected override string SeedFileName => "colosseum-avatar-decks.json";
}
