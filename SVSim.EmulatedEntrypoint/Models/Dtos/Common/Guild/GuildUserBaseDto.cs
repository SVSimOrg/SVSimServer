using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

/// <summary>
/// UserInfoBase flat-spread fields as used in guild contexts (GuildMemberInfo.cs, invite lists, etc.).
/// viewer_id / name / emblem_id / country_code / rank / degree_id / is_friend? / is_friend_apply?.
/// Matches types.ts.md#user-profile-types UserInfoBase.
/// </summary>
[MessagePackObject]
public class GuildUserBaseDto
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

    /// <summary>Optional — omit when the caller has no friend relationship with this user.</summary>
    [JsonPropertyName("is_friend"), Key("is_friend")]
    public int? IsFriend { get; set; }

    /// <summary>Optional — omit when the caller has no pending friend apply to this user.</summary>
    [JsonPropertyName("is_friend_apply"), Key("is_friend_apply")]
    public int? IsFriendApply { get; set; }
}
