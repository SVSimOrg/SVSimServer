using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/remove. Kick a member (leader/sub-leader only).</summary>
[MessagePackObject]
public class GuildRemoveRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>ViewerId of the member to remove. Field is `remove_viewer_id`, not `viewer_id`.</summary>
    [JsonPropertyName("remove_viewer_id"), Key("remove_viewer_id")]
    public long RemoveViewerId { get; set; }
}
