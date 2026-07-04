using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

/// <summary>
/// GuildMember wire shape — extends UserInfoBase (flat-spread) with is_official_mark_displayed + role.
/// Matches GuildMemberInfo.cs:43-50 and types.ts.md#guild-types GuildMember.
/// role: 0 = REGULAR, 1 = LEADER, 2 = SUB_LEADER.
/// </summary>
[MessagePackObject]
public class GuildMemberInfoDto
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

    /// <summary>
    /// Client reads unconditionally in GuildMemberInfo.cs:48 (json["is_friend"].ToBoolean()) — always emit.
    /// Base UserInfoBase guards this, but GuildMemberInfo re-reads it on top, unguarded. 0 = not a friend.
    /// </summary>
    [JsonPropertyName("is_friend"), Key("is_friend"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int IsFriend { get; set; }

    /// <summary>
    /// Client reads unconditionally in GuildMemberInfo.cs:49 (json["is_friend_apply"].ToBoolean()) — always emit.
    /// 0 = no pending friend apply.
    /// </summary>
    [JsonPropertyName("is_friend_apply"), Key("is_friend_apply"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int IsFriendApply { get; set; }

    /// <summary>
    /// Whether the official-account badge is shown for this member.
    /// Client reads this unconditionally from GuildMemberInfo.Parse — always emit it.
    /// </summary>
    [JsonPropertyName("is_official_mark_displayed"), Key("is_official_mark_displayed"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int IsOfficialMarkDisplayed { get; set; }

    /// <summary>0 = REGULAR, 1 = LEADER, 2 = SUB_LEADER. Stringified.</summary>
    [JsonPropertyName("role"), Key("role"), JsonConverter(typeof(StringifiedIntConverter))]
    public int Role { get; set; }
}
