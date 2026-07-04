using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

/// <summary>
/// Mirrors GuildDetailInfo.cs:143-156. Wire ints are emitted as stringified.
/// member_num is included on the wire only when this dto represents a search-result row
/// (the /guild/info "detail" inside `guild` object also includes it).
/// </summary>
[MessagePackObject]
public class GuildDetailDto
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
}
