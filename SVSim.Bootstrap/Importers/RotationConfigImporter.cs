using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using static SVSim.Bootstrap.Importers.ImporterBase;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Writes three <c>GameConfigSection</c> rows from the load-index seed split:
/// <c>seeds/rotation-config.json</c> → Rotation, <c>seeds/challenge-config.json</c> → Challenge,
/// <c>seeds/my-rotation-schedule.json</c> → MyRotationSchedule. Atomic section pattern: read the
/// existing section row (or shipped defaults), mutate the deserialized POCO, write back to
/// <c>ValueJson</c>. Re-runnable; rows missing from the seed leave the section row untouched.
/// </summary>
public class RotationConfigImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        int touched = 0;

        var rot = SeedLoader.LoadObject<RotationConfigSeed>(Path.Combine(seedDir, "rotation-config.json"));
        if (rot is not null)
        {
            await UpsertSection<RotationConfig>(context, RotationConfig.ShippedDefaults, c =>
            {
                c.TsRotationId = rot.TsRotationId;
                c.IsBattlePassPeriod = rot.IsBattlePassPeriod;
                c.IsBeginnerMission = rot.IsBeginnerMission;
                c.CardSetIdForResourceDlView = rot.CardSetIdForResourceDlView;
                c.RotationCardSetIds = rot.RotationCardSetIds ?? new List<int>();
            });
            touched++;
        }

        var cc = SeedLoader.LoadObject<ChallengeConfigSeed>(Path.Combine(seedDir, "challenge-config.json"));
        if (cc is not null)
        {
            await UpsertSection<ChallengeConfig>(context, ChallengeConfig.ShippedDefaults, c =>
            {
                c.LastCardPackSetId = cc.LastCardPackSetId;
                c.CardPoolName = cc.CardPoolName;
                c.CardPoolUrl = cc.CardPoolUrl;
                c.AnnounceId = cc.AnnounceId;
                c.StartTime = cc.StartTime;
                c.EndTime = cc.EndTime;
                c.TwoPickType = cc.TwoPickType;
                c.StrategyPickNum = cc.StrategyPickNum;
                c.PoolCardSetIds = cc.PoolCardSetIds ?? new List<int>();
            });
            touched++;
        }

        var schedule = SeedLoader.LoadObject<MyRotationScheduleSeed>(Path.Combine(seedDir, "my-rotation-schedule.json"));
        if (schedule?.Gathering is not null && schedule.FreeBattle is not null)
        {
            // Schedule windows are intentionally parsed WITHOUT AssumeUniversal because the seed
            // strings ("2024-05-01 20:00:00") are timezone-less and the rest of the pipeline (the
            // [ConfigSection] JSON round-trip + LoadController's wire mapping) treats them as
            // local-kind ticks. Mirrors the legacy GlobalsImporter.TryParseScheduleWindow behavior
            // — see GlobalsRepositoryTests for the round-trip assertion.
            var gBegin = ParseScheduleWireDateTime(schedule.Gathering.Begin);
            var gEnd = ParseScheduleWireDateTime(schedule.Gathering.End);
            var fBegin = ParseScheduleWireDateTime(schedule.FreeBattle.Begin);
            var fEnd = ParseScheduleWireDateTime(schedule.FreeBattle.End);
            // Only commit when both windows parsed to real DateTimes — a malformed/0001 value
            // would silently lock the MyRotation feature off (the original bug the section fixed).
            if (gBegin != DateTime.MinValue && gEnd != DateTime.MinValue
                && fBegin != DateTime.MinValue && fEnd != DateTime.MinValue)
            {
                await UpsertSection<MyRotationScheduleConfig>(context, MyRotationScheduleConfig.ShippedDefaults, c =>
                {
                    c.Gathering = new ScheduleWindow { Begin = gBegin, End = gEnd };
                    c.FreeBattle = new ScheduleWindow { Begin = fBegin, End = fEnd };
                });
                touched++;
            }
            else
            {
                Console.Error.WriteLine("[RotationConfigImporter] my-rotation-schedule.json windows malformed — keeping existing/shipped MyRotationSchedule.");
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[RotationConfigImporter] sections={touched}");
        return touched;
    }

    // Legacy schedule-window parse: default styles (AssumeLocal), matching the original
    // GlobalsImporter.TryParseScheduleWindow. The schedule strings are timezone-less; preserving
    // legacy local-kind ticks keeps the wire output byte-equivalent across the migration.
    private static DateTime ParseScheduleWireDateTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return DateTime.MinValue;
        return DateTime.TryParse(s, out var dt) ? dt : DateTime.MinValue;
    }

    // Verbatim copy of GlobalsImporter.UpsertSection<T>. Kept private-static here so this
    // importer can stand alone after Stage 9C strips the GlobalsImporter copy.
    private static async Task UpsertSection<T>(SVSimDbContext context, Func<T> shippedDefaults, Action<T> mutate)
        where T : class, new()
    {
        var sectionName = typeof(T).GetCustomAttributes(typeof(ConfigSectionAttribute), inherit: false)
            .Cast<ConfigSectionAttribute>().FirstOrDefault()?.Name
            ?? throw new InvalidOperationException($"{typeof(T).Name} is missing [ConfigSection].");

        var row = await context.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == sectionName);
        T value;
        if (row is null)
        {
            value = shippedDefaults();
            row = new GameConfigSection { SectionName = sectionName };
            context.GameConfigs.Add(row);
        }
        else
        {
            value = JsonSerializer.Deserialize<T>(row.ValueJson) ?? shippedDefaults();
        }
        mutate(value);
        row.ValueJson = JsonSerializer.Serialize(value);
    }
}
