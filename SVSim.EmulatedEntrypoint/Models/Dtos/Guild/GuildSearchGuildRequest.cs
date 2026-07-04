using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Request for POST /guild/search_guild.</summary>
[MessagePackObject]
public class GuildSearchGuildRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Name query. Empty string = match-all.</summary>
    [JsonPropertyName("guild_name"), Key("guild_name")]
    public string GuildName { get; set; } = "";

    /// <summary>ActivityType filter (1..16). 0 = any.</summary>
    [JsonPropertyName("activity"), Key("activity")]
    public int Activity { get; set; }

    /// <summary>JoinConditionType filter (1..3). 0 = any.</summary>
    [JsonPropertyName("join_condition"), Key("join_condition")]
    public int JoinCondition { get; set; }

    /// <summary>Member-count bucket filter. 0 = any, 1 = small, 2 = medium, 3 = large.</summary>
    [JsonPropertyName("member_condition_range"), Key("member_condition_range")]
    public int MemberConditionRange { get; set; }
}
