using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class EmblemIdentifier
{
    [JsonPropertyName("emblem_id")]
    [Key("emblem_id")]
    public long EmblemId { get; set; }
}