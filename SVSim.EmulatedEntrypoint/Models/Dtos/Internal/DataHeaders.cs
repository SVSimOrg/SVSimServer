using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Internal;

[MessagePackObject]
public class DataHeaders
{
    [JsonPropertyName("short_udid")]
    [Key("short_udid")]
    public long ShortUdid { get; set; }
    [JsonPropertyName("viewer_id")]
    [Key("viewer_id")]
    public long ViewerId { get; set; }
    [JsonPropertyName("sid")]
    [Key("sid")]
    public string Sid { get; set; }
    [JsonPropertyName("servertime")]
    [Key("servertime")]
    public long Servertime { get; set; }
    [JsonPropertyName("result_code")]
    [Key("result_code")]
    public int ResultCode { get; set; }

    /// <summary>
    /// Echoed UDID. Read by <c>SignUpTask.Parse</c> to validate response identity (client logs
    /// <c>udid一致しません</c> and discards the response on mismatch); ignored by every other
    /// client task. Always set by <c>ShadowverseTranslationMiddleware</c> from the request's
    /// resolved UDID — never from controller state. Empty string when the SID→UDID lookup misses
    /// (request without UDID/SID headers).
    /// </summary>
    [JsonPropertyName("udid")]
    [Key("udid")]
    public string Udid { get; set; } = "";

    /// <summary>
    /// Tells the client the required version path component for asset manifests on the
    /// resource server (Akamai CDN, hardcoded to <c>shadowverse.akamaized.net/</c> in
    /// <c>Wizard/SetUp.cs:48</c>). <c>NetworkTask.setResourceVersion</c> writes the value
    /// to <c>PlayerPrefs["RES_VER"]</c>; the manifest URL becomes
    /// <c>dl/Manifest/&lt;RES_VER&gt;/&lt;lang&gt;/&lt;Platform&gt;/</c>. When the client
    /// has no cached <c>RES_VER</c> (e.g., after <c>NukeIdentityOnStartup</c> wipes
    /// PlayerPrefs), it defaults to <c>"00000000"</c>, which Akamai doesn't serve — the
    /// manifest fetch 404s and the client shows "Connection Error / Reconnect" before
    /// the tutorial UI ever appears.
    /// <para>
    /// Nullable to keep it off the wire on responses that don't need it (the global
    /// <c>WhenWritingNull</c> policy in Program.cs handles the omission).
    /// </para>
    /// </summary>
    [JsonPropertyName("required_res_ver")]
    [Key("required_res_ver")]
    public string? RequiredResVer { get; set; }
}
