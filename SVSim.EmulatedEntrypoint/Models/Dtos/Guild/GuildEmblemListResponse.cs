using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Response for POST /guild/emblem_list. List of emblem entries the leader can select.</summary>
[MessagePackObject]
public class GuildEmblemListResponse
{
    [JsonPropertyName("guild_emblem_list"), Key("guild_emblem_list")]
    public List<GuildEmblemEntry> EmblemList { get; set; } = new();
}

/// <summary>One emblem entry. emblem_id is long per spec.</summary>
[MessagePackObject]
public class GuildEmblemEntry
{
    [JsonPropertyName("emblem_id"), Key("emblem_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long EmblemId { get; set; }
}
