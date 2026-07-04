using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>Response for POST /guild/search_guild. Flat list of guild details matching the filter.</summary>
[MessagePackObject]
public class GuildSearchGuildResponse
{
    [JsonPropertyName("list"), Key("list")]
    public List<GuildDetailDto> List { get; set; } = new();
}
