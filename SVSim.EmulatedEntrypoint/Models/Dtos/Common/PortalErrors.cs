using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// The portal (shadowverse-portal.com) wraps every response with an `errors` object that is
/// present even on success — the success-path payload carries a stub `UNKNOWN_ERROR` / "error
/// message" pair that the client ignores when result_code == 1. See
/// <c>docs/api-spec/endpoints/deck-builder/*.md</c>.
/// </summary>
[MessagePackObject]
public class PortalErrors
{
    [JsonPropertyName("type")]
    [Key("type")]
    public string Type { get; set; } = "UNKNOWN_ERROR";

    [JsonPropertyName("message")]
    [Key("message")]
    public string Message { get; set; } = "";
}
