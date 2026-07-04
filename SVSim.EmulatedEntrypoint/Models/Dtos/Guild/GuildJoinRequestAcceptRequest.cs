using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Request for POST /guild/join_request_accept.
/// GuildJoinRequestActionParam shape — single field request_viewer_id.
/// </summary>
[MessagePackObject]
public class GuildJoinRequestAcceptRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>ViewerId of the user whose join request is being accepted.</summary>
    [JsonPropertyName("request_viewer_id"), Key("request_viewer_id")]
    public long RequestViewerId { get; set; }
}
