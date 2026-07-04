using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Campaign;

/// <summary>
/// Body of <c>POST /campaign/regist_serial_code</c>. Client task:
/// <c>MyPageCodeInputTask</c> (Shadowverse_Code_2026-05-23/Wizard/MyPageCodeInputTask.cs).
/// </summary>
[MessagePackObject]
public sealed class RegisterSerialCodeRequest
{
    /// <summary>User-typed serial code. Case-sensitive on the server.</summary>
    [JsonPropertyName("serial_code")]
    [Key("serial_code")]
    public string SerialCode { get; set; } = string.Empty;
}
