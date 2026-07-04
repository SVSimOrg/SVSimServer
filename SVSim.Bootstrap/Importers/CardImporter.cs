using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Reads the loader's card dump (LitJson array of CardCSVData) and upserts ShadowverseCardEntry +
/// ShadowverseCardSetEntry rows. Idempotent.
/// </summary>
public class CardImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public async Task<int> ImportAsync(SVSimDbContext context, string cardsJsonPath)
    {
        if (!File.Exists(cardsJsonPath))
        {
            Console.Error.WriteLine($"[CardImporter] cards.json not found at {cardsJsonPath}; skipping card import.");
            return 0;
        }

        Console.WriteLine($"[CardImporter] Reading {cardsJsonPath} ({new FileInfo(cardsJsonPath).Length / 1024} KiB)...");

        List<CardInput>? input;
        await using (var fs = File.OpenRead(cardsJsonPath))
        {
            input = await JsonSerializer.DeserializeAsync<List<CardInput>>(fs, JsonOptions);
        }
        if (input is null || input.Count == 0)
        {
            Console.Error.WriteLine("[CardImporter] No card records parsed from input.");
            return 0;
        }
        Console.WriteLine($"[CardImporter] Parsed {input.Count} card records.");

        var classesById = await context.Classes.ToDictionaryAsync(c => c.Id);
        var existingSets = (await context.CardSets.ToListAsync()).ToDictionary(s => s.Id);
        var existingCards = (await context.Cards.ToListAsync()).ToDictionary(c => c.Id);
        Console.WriteLine(
            $"[CardImporter] DB state before: {existingCards.Count} cards, {existingSets.Count} card sets, " +
            $"{classesById.Count} classes seeded.");

        int created = 0, updated = 0, skipped = 0, setsCreated = 0;

        foreach (var c in input)
        {
            if (!long.TryParse(c.CardId, out long id) || id == 0)
            {
                skipped++;
                continue;
            }

            int setId = ParseInt(c.CardSetId, 0);
            int clan = ParseInt(c.Clan, 0);
            int rarity = ParseInt(c.Rarity, 0);

            if (!existingSets.TryGetValue(setId, out var set))
            {
                set = new ShadowverseCardSetEntry
                {
                    Id = setId,
                    Name = $"Card Set {setId}",
                    IsInRotation = true,
                    IsBasic = setId == 10000
                };
                context.CardSets.Add(set);
                existingSets[setId] = set;
                setsCreated++;
            }

            ClassEntry? classEntry = clan > 0 && classesById.TryGetValue(clan, out var ce) ? ce : null;
            var collection = new CardCollectionInfo
            {
                CraftCost = ParseInt(c.UseRedEther, 0),
                DustReward = ParseInt(c.GetRedEther, 0)
            };

            bool isFoil = c.IsFoil == "1";

            if (existingCards.TryGetValue(id, out var card))
            {
                card.Rarity = (Rarity)rarity;
                card.PrimaryResourceCost = ParseNullableInt(c.Cost);
                card.Attack = ParseNullableInt(c.Atk);
                card.Defense = ParseNullableInt(c.Life);
                card.Class = classEntry;
                card.CollectionInfo = collection;
                card.IsFoil = isFoil;
                updated++;
            }
            else
            {
                card = new ShadowverseCardEntry
                {
                    Id = id,
                    Name = $"Card {id}",
                    Rarity = (Rarity)rarity,
                    PrimaryResourceCost = ParseNullableInt(c.Cost),
                    Attack = ParseNullableInt(c.Atk),
                    Defense = ParseNullableInt(c.Life),
                    Class = classEntry,
                    CollectionInfo = collection,
                    IsFoil = isFoil
                };
                set.Cards.Add(card);
                existingCards[id] = card;
                created++;
            }
        }

        Console.WriteLine(
            $"[CardImporter] Saving: +{created} cards, ~{updated} updated, +{setsCreated} card sets, " +
            $"skipped {skipped} (bad/missing card_id)...");

        await context.SaveChangesAsync();
        Console.WriteLine("[CardImporter] Done.");
        return created + updated;
    }

    private static int ParseInt(string? raw, int fallback) =>
        int.TryParse(raw, out int v) ? v : fallback;

    private static int? ParseNullableInt(string? raw) =>
        int.TryParse(raw, out int v) ? v : null;
}

/// <summary>
/// Lightweight projection over the CardCSVData fields we care about. The dump has many more
/// fields (PascalCase metadata + effect/voice/visual paths) — we ignore them; only the
/// snake_case CSV columns map here via the SnakeCaseLower naming policy.
/// </summary>
public class CardInput
{
    public string? CardId { get; set; }
    public string? CardSetId { get; set; }
    public string? Clan { get; set; }
    public string? Cost { get; set; }
    public string? Atk { get; set; }
    public string? Life { get; set; }
    public string? Rarity { get; set; }
    public string? GetRedEther { get; set; }
    public string? UseRedEther { get; set; }
    public string? IsFoil { get; set; }    // cards.json `is_foil` = "0" or "1"
}
