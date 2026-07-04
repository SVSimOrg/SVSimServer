using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

/// <summary>
/// <c>POST /tool/signup</c> request body. Spec:
/// <c>docs/api-spec/endpoints/pre-login/tool-signup.md</c>. Client source:
/// <c>Shadowverse_Code_2026-05-23/Cute/SignUpTask.cs</c> (LoginPostParams).
///
/// All fields are device telemetry; the server doesn't use them in v1 but still binds them so
/// the request shape matches the spec exactly.
/// </summary>
[MessagePackObject]
public class SignupRequest
{
    [JsonPropertyName("device_name")]
    [Key("device_name")]
    public string DeviceName { get; set; } = "";

    [JsonPropertyName("client_type")]
    [Key("client_type")]
    public string ClientType { get; set; } = "";

    [JsonPropertyName("os_version")]
    [Key("os_version")]
    public string OsVersion { get; set; } = "";

    [JsonPropertyName("app_version")]
    [Key("app_version")]
    public string AppVersion { get; set; } = "";

    [JsonPropertyName("resource_version")]
    [Key("resource_version")]
    public string ResourceVersion { get; set; } = "";

    [JsonPropertyName("carrier")]
    [Key("carrier")]
    public string Carrier { get; set; } = "";
}
