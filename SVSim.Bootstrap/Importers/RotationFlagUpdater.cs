using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Models.Config;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Reads <see cref="RotationConfig"/> from the GameConfigs table (populated by
/// <see cref="RotationConfigImporter"/>) and flips <c>CardSet.IsInRotation</c> to match.
/// Must run after RotationConfigImporter and CardImporter — CardSets missing from the DB
/// can't be promoted (we log a warning instead of failing — the rotation flag flip is non-fatal).
/// </summary>
public class RotationFlagUpdater
{
    public async Task<int> UpdateAsync(SVSimDbContext context)
    {
        var sectionName = typeof(RotationConfig).GetCustomAttributes(typeof(ConfigSectionAttribute), inherit: false)
            .Cast<ConfigSectionAttribute>().FirstOrDefault()?.Name
            ?? throw new InvalidOperationException("RotationConfig missing [ConfigSection]");

        var row = await context.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == sectionName);
        if (row is null)
        {
            Console.WriteLine("[RotationFlagUpdater] No Rotation section in GameConfigs; skipping.");
            return 0;
        }

        var cfg = JsonSerializer.Deserialize<RotationConfig>(row.ValueJson);
        if (cfg is null)
        {
            Console.WriteLine("[RotationFlagUpdater] Failed to deserialize RotationConfig; skipping.");
            return 0;
        }

        var rotationSet = (cfg.RotationCardSetIds ?? new List<int>()).ToHashSet();
        if (rotationSet.Count == 0)
        {
            Console.WriteLine("[RotationFlagUpdater] RotationCardSetIds empty; no flag changes.");
            return 0;
        }

        var allSets = await context.CardSets.ToListAsync();
        int updated = 0, missing = 0;
        foreach (var rid in rotationSet)
        {
            var set = allSets.FirstOrDefault(s => s.Id == rid);
            if (set is null) { missing++; continue; }
            if (!set.IsInRotation) { set.IsInRotation = true; updated++; }
        }
        // Demote sets not in the current rotation.
        foreach (var s in allSets.Where(s => s.IsInRotation && !rotationSet.Contains(s.Id)))
        {
            s.IsInRotation = false;
            updated++;
        }
        if (missing > 0)
            Console.Error.WriteLine($"[RotationFlagUpdater] Warning: {missing} rotation card_set_id(s) missing from CardSets — run CardImporter first.");

        await context.SaveChangesAsync();
        Console.WriteLine($"[RotationFlagUpdater] CardSet.IsInRotation ~{updated}");
        return updated;
    }
}
