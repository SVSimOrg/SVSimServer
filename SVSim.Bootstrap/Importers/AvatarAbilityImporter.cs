using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of Avatar (Hero) ability rows from <c>seeds/avatar-abilities.json</c>.
/// Keyed by leader_skin_id. Ability / passive-ability DSL strings are preserved verbatim.
/// </summary>
public class AvatarAbilityImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<AvatarAbilitySeed>(Path.Combine(seedDir, "avatar-abilities.json"));
        if (seed.Count == 0) return 0;

        var existing = await context.AvatarAbilities.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;
        foreach (var s in seed)
        {
            if (s.Id == 0) continue;
            var entry = existing.TryGetValue(s.Id, out var ex) ? ex : new AvatarAbilityEntry { Id = s.Id };
            entry.BattleStartFirstPlayerTurnBp = s.BattleStartFirstPlayerTurnBp;
            entry.BattleStartSecondPlayerTurnBp = s.BattleStartSecondPlayerTurnBp;
            entry.BattleStartMaxLife = s.BattleStartMaxLife;
            entry.AbilityCost = s.AbilityCost;
            entry.Ability = s.Ability;
            entry.PassiveAbility = s.PassiveAbility;
            entry.AbilityDesc = s.AbilityDesc;
            entry.PassiveAbilityDesc = s.PassiveAbilityDesc;
            if (ex is null) { context.AvatarAbilities.Add(entry); existing[s.Id] = entry; created++; }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[AvatarAbilityImporter] +{created}/~{updated}");
        return created + updated;
    }
}
