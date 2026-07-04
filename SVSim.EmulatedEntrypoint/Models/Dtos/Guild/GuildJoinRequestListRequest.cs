using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Request for POST /guild/join_request_list.
/// KeysetPagination shape — client always sends page=0, oldest_time=0.
/// </summary>
[MessagePackObject]
public class GuildJoinRequestListRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    [JsonPropertyName("page"), Key("page")]
    public int Page { get; set; }

    [JsonPropertyName("oldest_time"), Key("oldest_time")]
    public long OldestTime { get; set; }
}
