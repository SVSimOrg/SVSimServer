using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Request for POST /guild/cancel_join_request.
/// No fields — server identifies the pending request from the calling user's state.
/// Uses explicit DTO (not BaseRequest) so we can add fields if needed in later tasks.
/// </summary>
[MessagePackObject]
public class GuildCancelJoinRequestRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";
}
