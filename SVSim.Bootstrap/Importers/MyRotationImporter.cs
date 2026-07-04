using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of MyRotation reference data: settings (per rotation_id) +
/// abilities (per ability_id). Seeds come from <c>seeds/my-rotation-settings.json</c> and
/// <c>seeds/my-rotation-abilities.json</c>; the extractor pre-joins the original wire's three
/// dicts (setting, reprinted, restricted) on rotation_id, so the importer just iterates.
/// </summary>
public class MyRotationImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var settings = SeedLoader.LoadList<MyRotationSettingSeed>(Path.Combine(seedDir, "my-rotation-settings.json"));
        var abilities = SeedLoader.LoadList<MyRotationAbilitySeed>(Path.Combine(seedDir, "my-rotation-abilities.json"));

        if (settings.Count == 0 && abilities.Count == 0) return 0;

        int sCreated = 0, sUpdated = 0;
        var existingSettings = await context.MyRotationSettings.ToDictionaryAsync(e => e.Id);
        foreach (var s in settings)
        {
            if (s.Id == 0) continue;
            var entry = existingSettings.TryGetValue(s.Id, out var ex) ? ex : new MyRotationSettingEntry { Id = s.Id };
            entry.CardSetIdsCsv = s.CardSetIdsCsv;
            entry.AbilitiesCsv = s.AbilitiesCsv;
            entry.ReprintedCardIds = s.ReprintedCardIds;
            entry.RestrictedCardIds = s.RestrictedCardIds;
            if (ex is null) { context.MyRotationSettings.Add(entry); existingSettings[s.Id] = entry; sCreated++; }
            else sUpdated++;
        }

        int aCreated = 0, aUpdated = 0;
        var existingAbilities = await context.MyRotationAbilities.ToDictionaryAsync(e => e.Id);
        foreach (var s in abilities)
        {
            if (s.Id == 0) continue;
            var entry = existingAbilities.TryGetValue(s.Id, out var ex) ? ex : new MyRotationAbilityEntry { Id = s.Id };
            entry.Data = JsonSerializer.Serialize(s.Data);
            if (ex is null) { context.MyRotationAbilities.Add(entry); existingAbilities[s.Id] = entry; aCreated++; }
            else aUpdated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[MyRotationImporter] settings +{sCreated}/~{sUpdated}, abilities +{aCreated}/~{aUpdated}");
        return sCreated + sUpdated + aCreated + aUpdated;
    }
}
