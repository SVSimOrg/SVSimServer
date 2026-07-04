using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Models;

/// <summary>
/// Round-trip tests for the GameConfigs key/value table — one row per section, raw jsonb payload
/// deserialised by IGameConfigService rather than EF Core. Replaces the prior single-row
/// GameConfigurations / GameConfigRoot jsonb-tree shape (2026-05-24 refactor).
/// </summary>
public class GameConfigurationJsonbTests
{
    [Test]
    public async Task EnsureSeedData_writes_one_row_per_ConfigSection_with_ShippedDefaults_payload()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var rows = await db.GameConfigs.AsNoTracking().ToListAsync();
        var byName = rows.ToDictionary(r => r.SectionName);

        // One row per [ConfigSection]-marked POCO (19 sections today: Player, DefaultGrants,
        // DefaultLoadout, Challenge, Rotation, PackRates, MyRotationSchedule, Story, ResourceConfig,
        // Freeplay, ArenaTwoPick, Matching, CardMasterConfig, ColosseumSeason, ColosseumRounds,
        // LoginBonus, Guild, SkipTutorial, BattleXp).
        Assert.That(byName.Keys, Is.EquivalentTo(new[]
        {
            "Player", "DefaultGrants", "DefaultLoadout", "Challenge", "Rotation", "PackRates",
            "MyRotationSchedule", "Story", "ResourceConfig", "Freeplay", "ArenaTwoPick", "Matching",
            "CardMasterConfig", "ColosseumSeason", "ColosseumRounds", "LoginBonus", "Guild",
            "SkipTutorial", "BattleXp", "GameCalendar",
        }));

        var resources = JsonSerializer.Deserialize<ResourceConfig>(byName["ResourceConfig"].ValueJson)!;
        Assert.That(resources.RequiredResVer, Is.EqualTo("4670rPsPMVlRTd2"),
            "ShippedDefaults RES_VER is the prod-captured (2026-05-28) Akamai manifest path " +
            "— required by the client to load the asset manifest after a wiped/fresh install.");

        var mrSchedule = JsonSerializer.Deserialize<MyRotationScheduleConfig>(byName["MyRotationSchedule"].ValueJson)!;
        Assert.That(mrSchedule.FreeBattle.Begin, Is.EqualTo(new DateTime(2024, 5, 1, 20, 0, 0, DateTimeKind.Utc)),
            "ShippedDefaults reproduces the 2026-05-23 prod capture so a fresh install ships with Custom Rotation enabled");
        Assert.That(mrSchedule.FreeBattle.End, Is.EqualTo(new DateTime(2030, 6, 26, 19, 59, 59, DateTimeKind.Utc)));

        var packRates = JsonSerializer.Deserialize<PackRateConfig>(byName["PackRates"].ValueJson)!;
        Assert.That(packRates.AnimatedRate, Is.EqualTo(0.08).Within(1e-9), "SV Classic AnimatedRate");
        Assert.That(packRates.Default.Bronze, Is.EqualTo(0.6744).Within(1e-9));
        var slot8 = packRates.PerSlot.FirstOrDefault(s => s.Slot == "8");
        Assert.That(slot8, Is.Not.Null, "ShippedDefaults() includes the slot-8 Silver-or-better entry");
        Assert.That(slot8!.Silver, Is.EqualTo(0.7692).Within(1e-9));

        var grants = JsonSerializer.Deserialize<DefaultGrantsConfig>(byName["DefaultGrants"].ValueJson)!;
        Assert.That(grants.Crystals, Is.EqualTo(0UL));
        Assert.That(grants.Rupees,   Is.EqualTo(0UL));
        Assert.That(grants.Ether,    Is.EqualTo(0UL));

        var player = JsonSerializer.Deserialize<PlayerConfig>(byName["Player"].ValueJson)!;
        Assert.That(player.MaxFriends, Is.EqualTo(20));

        var loadout = JsonSerializer.Deserialize<DefaultLoadoutConfig>(byName["DefaultLoadout"].ValueJson)!;
        Assert.That(loadout.SleeveId, Is.EqualTo(3000011));
    }

    [Test]
    public async Task Section_row_round_trips_through_jsonb_via_raw_json()
    {
        using var factory = new SVSimTestFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var rotation = await db.GameConfigs.FirstAsync(s => s.SectionName == "Rotation");
            // Hydrate, mutate, re-serialise — the pattern RotationConfigImporter and any admin-write
            // path will use.
            var value = JsonSerializer.Deserialize<RotationConfig>(rotation.ValueJson)!;
            value.TsRotationId = "99999";
            value.IsBattlePassPeriod = true;
            rotation.ValueJson = JsonSerializer.Serialize(value);
            await db.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var rotation = await db.GameConfigs.FirstAsync(s => s.SectionName == "Rotation");
            var value = JsonSerializer.Deserialize<RotationConfig>(rotation.ValueJson)!;
            Assert.That(value.TsRotationId, Is.EqualTo("99999"));
            Assert.That(value.IsBattlePassPeriod, Is.True);
        }
    }

    /// <summary>
    /// Operator-edited PerSlot override (e.g. 100% Legendary for testing) must survive a DB
    /// round-trip and produce exactly ONE entry per slot — not stack on top of any default seed.
    /// The 2026-05-24 bug shape: pre-refactor PackRateConfig.PerSlot shipped with a Classic
    /// slot-8 seed in its initialiser; EF Core 8's OwnsMany jsonb path appended the operator's
    /// override on top instead of replacing it, and the seed won the FirstOrDefault in
    /// ResolveWeights. Post-refactor this can't happen (PerSlot defaults to empty,
    /// IGameConfigService uses pure STJ which replaces) but the round-trip assertion stays.
    /// </summary>
    [Test]
    public async Task Operator_PerSlot_override_round_trips_as_sole_entry_for_that_slot()
    {
        using var factory = new SVSimTestFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var row = await db.GameConfigs.FirstAsync(s => s.SectionName == "PackRates");
            var rates = JsonSerializer.Deserialize<PackRateConfig>(row.ValueJson)!;
            // Operator wipes the seeded slot-8 entry and replaces it with a 100%-Legendary override.
            rates.PerSlot.Clear();
            rates.PerSlot.Add(new SlotRarityWeights
            {
                Slot = "8", Bronze = 0, Silver = 0, Gold = 0, Legendary = 1,
            });
            row.ValueJson = JsonSerializer.Serialize(rates);
            await db.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var row = await db.GameConfigs.FirstAsync(s => s.SectionName == "PackRates");
            var rates = JsonSerializer.Deserialize<PackRateConfig>(row.ValueJson)!;

            var slot8Entries = rates.PerSlot.Where(s => s.Slot == "8").ToList();
            Assert.That(slot8Entries, Has.Count.EqualTo(1),
                "exactly one PerSlot[8] entry must round-trip — duplicates mean the loader appended " +
                "instead of replacing (the 2026-05-24 bug pattern).");
            Assert.That(slot8Entries[0].Legendary, Is.EqualTo(1.0).Within(1e-9),
                "the surviving PerSlot[8] entry must be the operator's override, not a stale seed.");
        }
    }

    [Test]
    public async Task RotationConfigImporter_updates_Rotation_without_clobbering_other_sections()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();   // imports load-index which has ts_rotation_id="10015"

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var rotation = JsonSerializer.Deserialize<RotationConfig>(
            (await db.GameConfigs.FirstAsync(s => s.SectionName == "Rotation")).ValueJson)!;
        Assert.That(rotation.TsRotationId, Is.EqualTo("10015"),
            "RotationConfigImporter should set Rotation.TsRotationId from the seed.");

        // PackRates is NOT in the load-index capture; its row must keep ShippedDefaults values.
        var packRates = JsonSerializer.Deserialize<PackRateConfig>(
            (await db.GameConfigs.FirstAsync(s => s.SectionName == "PackRates")).ValueJson)!;
        Assert.That(packRates.AnimatedRate, Is.EqualTo(0.08).Within(1e-9),
            "RotationConfigImporter must not clobber PackRates while updating Rotation.");
    }
}
