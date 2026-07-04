using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Emblem;

[MessagePackObject]
public class EmblemListEntry
{
    [JsonPropertyName("emblem_id")]
    [Key("emblem_id")]
    public int EmblemId { get; set; }
}
