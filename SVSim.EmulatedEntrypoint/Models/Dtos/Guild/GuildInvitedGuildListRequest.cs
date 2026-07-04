using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/invited_guild_list. Lists pending invites the user has received.</summary>
[MessagePackObject]
public class GuildInvitedGuildListRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Pagination page index. This list is the only one the client actually scrolls.</summary>
    [JsonPropertyName("page"), Key("page")]
    public int Page { get; set; }
}
