using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/create.</summary>
[MessagePackObject]
public class GuildCreateRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    [JsonPropertyName("guild_name"), Key("guild_name")]
    public string GuildName { get; set; } = "";

    /// <summary>GuildDetailInfo.ActivityType enum (1..16).</summary>
    [JsonPropertyName("activity"), Key("activity")]
    public int Activity { get; set; }

    /// <summary>GuildDetailInfo.JoinConditionType enum (1 FREE, 2 APPROVAL, 3 ONLY_INVITE).</summary>
    [JsonPropertyName("join_condition"), Key("join_condition")]
    public int JoinCondition { get; set; }
}
