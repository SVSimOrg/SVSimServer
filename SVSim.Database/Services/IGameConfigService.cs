namespace SVSim.Database.Services;

/// <summary>
/// Read-only access to game-domain configuration. Resolves each section atomically through the
/// tier chain: DB row in <c>GameConfigs</c> → <c>appsettings.json</c> section
/// <c>"GameConfig:&lt;SectionName&gt;"</c> → <c>T.ShippedDefaults()</c> → <c>new T()</c>.
/// <para>
/// "Atomic" means: the first tier that has the section wins entirely; tiers are not merged
/// per-property. This is deliberate — see 2026-05-24 config refactor discussion. Caching is
/// not implemented today (scoped lifetime; one DB read per request); the interface is shaped
/// to allow it to be added later without changing call sites.
/// </para>
/// </summary>
public interface IGameConfigService
{
    /// <summary>
    /// Resolves the section identified by <typeparamref name="T"/>'s
    /// <c>ConfigSectionAttribute</c>. Throws if the type is not annotated.
    /// </summary>
    T Get<T>() where T : class, new();
}
