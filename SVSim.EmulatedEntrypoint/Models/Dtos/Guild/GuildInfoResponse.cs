using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Mirrors GuildInfo.cs:68-86. Always-present: max_member_num, max_sub_leader_num,
/// guild_status, usable_stamp_list. Conditionally present (client `Keys.Contains` -gated):
/// guild (detail + members), join_request_count, invite_count.
/// </summary>
[MessagePackObject]
public class GuildInfoResponse
{
    [JsonPropertyName("max_member_num"), Key("max_member_num"), JsonConverter(typeof(StringifiedIntConverter)),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int MaxMemberNum { get; set; }

    [JsonPropertyName("max_sub_leader_num"), Key("max_sub_leader_num"), JsonConverter(typeof(StringifiedIntConverter)),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int MaxSubLeaderNum { get; set; }

    [JsonPropertyName("guild_status"), Key("guild_status"), JsonConverter(typeof(StringifiedIntConverter)),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int GuildStatus { get; set; }

    /// <summary>Stamp ids the viewer can use in chat. Stringified ints (capture: ["100001",...]).</summary>
    [JsonPropertyName("usable_stamp_list"), Key("usable_stamp_list"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<string> UsableStampList { get; set; } = new();

    [JsonPropertyName("guild"), Key("guild")]
    public GuildBundle? Guild { get; set; }

    [JsonPropertyName("join_request_count"), Key("join_request_count"), JsonConverter(typeof(StringifiedIntConverter))]
    public int? JoinRequestCount { get; set; }

    [JsonPropertyName("invite_count"), Key("invite_count"), JsonConverter(typeof(StringifiedIntConverter))]
    public int? InviteCount { get; set; }
}

[MessagePackObject]
public class GuildBundle
{
    [JsonPropertyName("detail"), Key("detail")]
    public GuildDetailDto Detail { get; set; } = new();

    [JsonPropertyName("members"), Key("members")]
    public List<GuildMemberInfoDto> Members { get; set; } = new();
}
