using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Singleton upsert (Id=1) of the active Take Two arena season config from
/// <c>seeds/arena-season.json</c>. <c>format_info</c> is preserved verbatim as a jsonb blob.
/// </summary>
public class ArenaSeasonImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var s = SeedLoader.LoadObject<ArenaSeasonSeed>(Path.Combine(seedDir, "arena-season.json"));
        if (s is null) return 0;

        var existing = await context.ArenaSeasons.FirstOrDefaultAsync(e => e.Id == 1);
        var entry = existing ?? new ArenaSeasonConfig { Id = 1 };
        entry.Mode = s.Mode;
        entry.Enable = s.Enable;
        entry.Cost = s.Cost;
        entry.RupyCost = s.RupyCost;
        entry.TicketCost = s.TicketCost;
        entry.IsJoin = s.IsJoin;
        entry.FormatInfo = s.FormatInfo.ValueKind == JsonValueKind.Undefined
            ? "{}"
            : JsonSerializer.Serialize(s.FormatInfo);
        if (existing is null) context.ArenaSeasons.Add(entry);

        await context.SaveChangesAsync();
        Console.WriteLine($"[ArenaSeasonImporter] {(existing is null ? "+1" : "~1")}");
        return 1;
    }
}
