using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// One entry in the /guild/friend_list response.
/// GuildFriendListTask.Parse() iterates base.ResponseData["data"][i] directly — data is a
/// bare JSON array, not a wrapper object. This DTO is the element type of that array.
/// </summary>
[MessagePackObject]
public class GuildInviteCandidateDto
{
    [JsonPropertyName("viewer_id"), Key("viewer_id"), JsonConverter(typeof(SVSim.EmulatedEntrypoint.Models.Dtos.Common.StringifiedLongConverter))]
    public long ViewerId { get; set; }

    [JsonPropertyName("name"), Key("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("emblem_id"), Key("emblem_id"), JsonConverter(typeof(SVSim.EmulatedEntrypoint.Models.Dtos.Common.StringifiedLongConverter))]
    public long EmblemId { get; set; }

    [JsonPropertyName("country_code"), Key("country_code")]
    public string CountryCode { get; set; } = "";

    [JsonPropertyName("rank"), Key("rank"), JsonConverter(typeof(SVSim.EmulatedEntrypoint.Models.Dtos.Common.StringifiedIntConverter))]
    public int Rank { get; set; }

    [JsonPropertyName("degree_id"), Key("degree_id"), JsonConverter(typeof(SVSim.EmulatedEntrypoint.Models.Dtos.Common.StringifiedIntConverter))]
    public int DegreeId { get; set; }

    [JsonPropertyName("is_friend"), Key("is_friend")]
    public int? IsFriend { get; set; }

    [JsonPropertyName("is_friend_apply"), Key("is_friend_apply")]
    public int? IsFriendApply { get; set; }

    /// <summary>true => friend is already in a guild and can't be invited.</summary>
    [JsonPropertyName("is_join_guild"), Key("is_join_guild")]
    public bool IsJoinGuild { get; set; }
}
