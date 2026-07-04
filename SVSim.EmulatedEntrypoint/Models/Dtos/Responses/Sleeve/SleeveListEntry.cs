using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Sleeve;

[MessagePackObject]
public class SleeveListEntry
{
    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")]
    public long SleeveId { get; set; }
}
