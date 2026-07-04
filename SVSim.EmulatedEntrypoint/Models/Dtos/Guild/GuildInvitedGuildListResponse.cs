using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Response for POST /guild/invited_guild_list.
/// Each entry is GuildDetail fields + invite_id as flat siblings on the wire.
/// Per spec: client constructs GuildDetailInfo + reads invite_id from the SAME object.
/// </summary>
[MessagePackObject]
public class GuildInvitedGuildListResponse
{
    [JsonPropertyName("list"), Key("list")]
    public List<GuildReceivedInviteDto> List { get; set; } = new();
}

/// <summary>
/// ReceivedInvite — GuildDetail fields flat-spread + invite_id.
/// invite_id and detail fields are siblings, not nested (per guild-invited_guild_list.md).
/// </summary>
[MessagePackObject]
public class GuildReceivedInviteDto
{
    [JsonPropertyName("guild_id"), Key("guild_id"), JsonConverter(typeof(StringifiedIntConverter))]
    public int GuildId { get; set; }

    [JsonPropertyName("guild_name"), Key("guild_name")]
    public string GuildName { get; set; } = "";

    [JsonPropertyName("description"), Key("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("guild_emblem_id"), Key("guild_emblem_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long GuildEmblemId { get; set; }

    [JsonPropertyName("join_condition"), Key("join_condition"), JsonConverter(typeof(StringifiedIntConverter))]
    public int JoinCondition { get; set; }

    [JsonPropertyName("activity"), Key("activity"), JsonConverter(typeof(StringifiedIntConverter))]
    public int Activity { get; set; }

    [JsonPropertyName("member_num"), Key("member_num"), JsonConverter(typeof(StringifiedIntConverter))]
    public int MemberNum { get; set; }

    [JsonPropertyName("leader_name"), Key("leader_name")]
    public string LeaderName { get; set; } = "";

    [JsonPropertyName("leader_viewer_id"), Key("leader_viewer_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long LeaderViewerId { get; set; }

    /// <summary>Invite id — pass to /guild/join (from_invite=true) or /guild/reject_invite.</summary>
    [JsonPropertyName("invite_id"), Key("invite_id")]
    public long InviteId { get; set; }
}
