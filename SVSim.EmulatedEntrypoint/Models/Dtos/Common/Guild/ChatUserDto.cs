using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

/// <summary>
/// Generic UserInfoBase shape for chat contexts — used in /guild_chat/messages `users[]`
/// and shared with /gathering_chat. Matches types.ts.md#user-profile-types ChatUser (UserInfoBase).
/// </summary>
[MessagePackObject]
public class ChatUserDto
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

    /// <summary>Optional — omit when the viewer has no friend relationship with this user.</summary>
    [JsonPropertyName("is_friend"), Key("is_friend")]
    public int? IsFriend { get; set; }

    /// <summary>Optional — omit when no pending friend apply.</summary>
    [JsonPropertyName("is_friend_apply"), Key("is_friend_apply")]
    public int? IsFriendApply { get; set; }
}
