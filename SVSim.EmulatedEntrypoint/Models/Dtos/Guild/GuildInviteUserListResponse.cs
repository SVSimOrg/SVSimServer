using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Response for POST /guild/invite_user_list.
/// Lists outstanding invites the guild has sent out (leader/sub-leader view).
/// </summary>
[MessagePackObject]
public class GuildInviteUserListResponse
{
    [JsonPropertyName("list"), Key("list")]
    public List<GuildOutgoingInviteDto> Users { get; set; } = new();
}

/// <summary>
/// OutgoingInvite — UserInfoBase fields flat-spread + invite_id + invite_time.
/// Matches guild-invite_user_list.md OutgoingInvite interface.
/// </summary>
[MessagePackObject]
public class GuildOutgoingInviteDto
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

    /// <summary>Invite id — pass to /guild/cancel_invite to retract.</summary>
    [JsonPropertyName("invite_id"), Key("invite_id")]
    public long InviteId { get; set; }

    /// <summary>Unix seconds when the invite was sent.</summary>
    [JsonPropertyName("invite_time"), Key("invite_time")]
    public long InviteTime { get; set; }
}
