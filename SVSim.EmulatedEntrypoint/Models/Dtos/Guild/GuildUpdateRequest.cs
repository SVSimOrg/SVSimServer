using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/update. Leader-only.</summary>
[MessagePackObject]
public class GuildUpdateRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    [JsonPropertyName("guild_name"), Key("guild_name")]
    public string GuildName { get; set; } = "";

    /// <summary>ActivityType enum (1..16).</summary>
    [JsonPropertyName("activity"), Key("activity")]
    public int Activity { get; set; }

    /// <summary>JoinConditionType enum (1..3).</summary>
    [JsonPropertyName("join_condition"), Key("join_condition")]
    public int JoinCondition { get; set; }
}
