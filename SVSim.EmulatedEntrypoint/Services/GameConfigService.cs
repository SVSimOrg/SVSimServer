using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SVSim.Database;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Three-tier resolver for <see cref="IGameConfigService"/>: GameConfigs row → IConfiguration
/// section under "GameConfig:&lt;name&gt;" → <c>T.ShippedDefaults()</c> (via reflection) → <c>new T()</c>.
/// <para>
/// Atomic per section: the first tier that has the section wins entirely; no per-property merging.
/// Scoped lifetime — one DB read per request — matches today's <c>GameConfigRoot</c> behavior.
/// </para>
/// </summary>
public class GameConfigService : IGameConfigService
{
    private static readonly ConcurrentDictionary<Type, SectionMetadata> _metaCache = new();

    private readonly SVSimDbContext _db;
    private readonly IConfiguration _appSettings;

    public GameConfigService(SVSimDbContext db, IConfiguration appSettings)
    {
        _db = db;
        _appSettings = appSettings;
    }

    public T Get<T>() where T : class, new()
    {
        var meta = GetMeta(typeof(T));

        // Tier 1: DB row
        var row = _db.GameConfigs.AsNoTracking().FirstOrDefault(s => s.SectionName == meta.SectionName);
        if (row is not null)
        {
            return (T?)JsonSerializer.Deserialize(row.ValueJson, typeof(T))
                ?? throw new InvalidOperationException(
                    $"GameConfigs row '{meta.SectionName}' deserialised to null — corrupt jsonb?");
        }

        // Tier 2: appsettings.json under "GameConfig:<name>"
        var configSection = _appSettings.GetSection($"GameConfig:{meta.SectionName}");
        if (configSection.Exists())
        {
            var fromAppsettings = configSection.Get<T>();
            if (fromAppsettings is not null) return fromAppsettings;
        }

        // Tier 3: ShippedDefaults() if present, else parameterless ctor
        if (meta.ShippedDefaultsFactory is not null)
        {
            return (T)meta.ShippedDefaultsFactory.Invoke(null, null)!;
        }
        return new T();
    }

    private static SectionMetadata GetMeta(Type t) => _metaCache.GetOrAdd(t, static type =>
    {
        var attr = type.GetCustomAttribute<ConfigSectionAttribute>(inherit: false)
            ?? throw new InvalidOperationException(
                $"{type.FullName} is not marked with [ConfigSection(...)] — IGameConfigService can't resolve it.");
        var factory = type.GetMethod("ShippedDefaults",
            BindingFlags.Public | BindingFlags.Static,
            binder: null, types: Type.EmptyTypes, modifiers: null);
        return new SectionMetadata(attr.Name, factory);
    });

    private sealed record SectionMetadata(string SectionName, MethodInfo? ShippedDefaultsFactory);
}
