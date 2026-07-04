namespace SVSim.Database.Models.Config;

/// <summary>
/// Asset-delivery tunables: where the client looks for the resource CDN (Akamai by default;
/// <c>Wizard/SetUp.cs:48</c> hardcodes <c>shadowverse.akamaized.net/</c>) and what manifest
/// version to ask for. Currently a single field, will grow as we self-host content.
/// </summary>
[ConfigSection("ResourceConfig")]
public class ResourceConfig
{
    /// <summary>
    /// Pushed to the client as <c>data_headers.required_res_ver</c>. The client writes it to
    /// <c>PlayerPrefs["RES_VER"]</c> and uses it as the version path component for asset
    /// manifest lookups: <c>https://&lt;cdn&gt;/dl/Manifest/&lt;RES_VER&gt;/&lt;lang&gt;/&lt;Platform&gt;/</c>.
    /// <para>
    /// Default value is the prod-captured version from <c>data_dumps/captures/traffic_prod_tutorial.ndjson</c>
    /// (2026-05-28) — i.e., a path Akamai actually serves. When this rotates (or Akamai sunsets
    /// ahead of June 2026), update via DB <c>GameConfigs</c> row, appsettings.json, or this
    /// shipped default; no code change needed.
    /// </para>
    /// <para>
    /// When the client has no cached <c>RES_VER</c> (e.g., a wiped/fresh install via
    /// <c>NukeIdentityOnStartup</c>), it defaults to <c>"00000000"</c>, which Akamai 404s. The
    /// fetch failure surfaces as "Connection Error / Reconnect" before any tutorial UI loads,
    /// so emitting a valid value here is required for fresh-account boot.
    /// </para>
    /// </summary>
    public string RequiredResVer { get; set; } = "4670rPsPMVlRTd2";

    /// <summary>
    /// Inline-default tier for <see cref="IGameConfigService"/>. Mirrors property initialisers
    /// — kept as a separate factory because the framework requires every [ConfigSection] POCO to
    /// expose one (see <c>feedback_config_defaults</c> memory for the collection-defaults rule
    /// that motivated the convention).
    /// </summary>
    public static ResourceConfig ShippedDefaults() => new();
}
