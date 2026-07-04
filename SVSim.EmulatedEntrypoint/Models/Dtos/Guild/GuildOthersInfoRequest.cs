using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/others_info. Browse another guild's public detail.</summary>
[MessagePackObject]
public class GuildOthersInfoRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    [JsonPropertyName("guild_id"), Key("guild_id"), JsonConverter(typeof(StringifiedIntConverter))]
    public int GuildId { get; set; }
}
