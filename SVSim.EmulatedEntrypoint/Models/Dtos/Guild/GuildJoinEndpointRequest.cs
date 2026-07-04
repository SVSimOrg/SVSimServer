using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Request for POST /guild/join.
/// NOTE: named GuildJoinEndpointRequest (not GuildJoinRequest) to avoid collision with
/// SVSim.Database.Entities.Guild.GuildJoinRequest which is an EF entity.
/// </summary>
[MessagePackObject]
public class GuildJoinEndpointRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    [JsonPropertyName("guild_id"), Key("guild_id"), JsonConverter(typeof(StringifiedIntConverter))]
    public int GuildId { get; set; }

    /// <summary>true = consuming an open invite (from /guild/invited_guild_list).</summary>
    [JsonPropertyName("from_invite"), Key("from_invite")]
    public bool FromInvite { get; set; }
}
