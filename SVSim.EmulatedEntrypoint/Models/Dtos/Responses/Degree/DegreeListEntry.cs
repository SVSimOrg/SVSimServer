using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Degree;

[MessagePackObject]
public class DegreeListEntry
{
    [JsonPropertyName("degree_id")]
    [Key("degree_id")]
    public int DegreeId { get; set; }
}
