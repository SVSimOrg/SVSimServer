using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Response for POST /guild/others_info. Only the detail sub-tree — no member list (privacy).
/// Reuses GuildDetailSubTree (same shape as /guild/update response).
/// </summary>
[MessagePackObject]
public class GuildOthersInfoResponse
{
    [JsonPropertyName("guild"), Key("guild")]
    public GuildDetailSubTree Guild { get; set; } = new();
}
