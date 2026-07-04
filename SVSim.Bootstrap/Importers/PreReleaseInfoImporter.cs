using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;
using static SVSim.Bootstrap.Importers.ImporterBase;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Singleton upsert (Id=1) of the pre-release window from <c>seeds/pre-release-info.json</c>.
/// Card-id list / dict blobs are preserved verbatim into their jsonb columns; date strings go
/// through <see cref="ImporterBase.ParseWireDateTime"/>.
/// </summary>
public class PreReleaseInfoImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var s = SeedLoader.LoadObject<PreReleaseInfoSeed>(Path.Combine(seedDir, "pre-release-info.json"));
        if (s is null) return 0;

        var existing = await context.PreReleaseInfos.FirstOrDefaultAsync(e => e.Id == 1);
        var entry = existing ?? new PreReleaseInfo { Id = 1 };
        entry.PreReleaseId = s.PreReleaseId;
        entry.NextCardSetId = s.NextCardSetId;
        entry.StartTime = ParseWireDateTime(s.StartTime);
        entry.EndTime = ParseWireDateTime(s.EndTime);
        entry.DisplayEndTime = ParseWireDateTime(s.DisplayEndTime);
        entry.FreeMatchStartTime = ParseWireDateTime(s.FreeMatchStartTime);
        entry.CardMasterId = s.CardMasterId;
        entry.DefaultCardMasterId = s.DefaultCardMasterId;
        entry.PreReleaseCardMasterId = s.PreReleaseCardMasterId;
        entry.IsPreRotationFreeMatchTerm = s.IsPreRotationFreeMatchTerm;
        entry.RotationCardSetIdList = s.RotationCardSetIdList.ValueKind == JsonValueKind.Undefined
            ? "[]" : JsonSerializer.Serialize(s.RotationCardSetIdList);
        entry.ReprintedBaseCardIds = s.ReprintedBaseCardIds.ValueKind == JsonValueKind.Undefined
            ? "{}" : JsonSerializer.Serialize(s.ReprintedBaseCardIds);
        entry.LatestReprintedBaseCardIds = s.LatestReprintedBaseCardIds.ValueKind == JsonValueKind.Undefined
            ? "{}" : JsonSerializer.Serialize(s.LatestReprintedBaseCardIds);
        if (existing is null) context.PreReleaseInfos.Add(entry);

        await context.SaveChangesAsync();
        Console.WriteLine($"[PreReleaseInfoImporter] {(existing is null ? "+1" : "~1")}");
        return 1;
    }
}
