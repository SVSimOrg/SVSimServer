using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Response for POST /guild/join_request_list (leader/sub-leader view).</summary>
[MessagePackObject]
public class GuildJoinRequestListResponse
{
    [JsonPropertyName("list"), Key("list")]
    public List<GuildJoinRequestEntryDto> Users { get; set; } = new();
}

/// <summary>
/// JoinRequest entry — UserInfoBase fields flat-spread + is_official_mark_displayed + request_time.
/// Matches guild-join_request_list.md JoinRequest interface.
/// </summary>
[MessagePackObject]
public class GuildJoinRequestEntryDto
{
    [JsonPropertyName("viewer_id"), Key("viewer_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long ViewerId { get; set; }

    [JsonPropertyName("name"), Key("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("emblem_id"), Key("emblem_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long EmblemId { get; set; }

    [JsonPropertyName("country_code"), Key("country_code")]
    public string CountryCode { get; set; } = "";

    [JsonPropertyName("rank"), Key("rank"), JsonConverter(typeof(StringifiedIntConverter))]
    public int Rank { get; set; }

    [JsonPropertyName("degree_id"), Key("degree_id"), JsonConverter(typeof(StringifiedIntConverter))]
    public int DegreeId { get; set; }

    [JsonPropertyName("is_friend"), Key("is_friend")]
    public int? IsFriend { get; set; }

    [JsonPropertyName("is_friend_apply"), Key("is_friend_apply")]
    public int? IsFriendApply { get; set; }

    [JsonPropertyName("is_official_mark_displayed"), Key("is_official_mark_displayed")]
    public int IsOfficialMarkDisplayed { get; set; }

    /// <summary>Unix seconds when the request was submitted. Don't send millisecond timestamps.</summary>
    [JsonPropertyName("request_time"), Key("request_time")]
    public long RequestTime { get; set; }
}
