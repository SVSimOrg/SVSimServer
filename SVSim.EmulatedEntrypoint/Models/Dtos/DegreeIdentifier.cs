using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class DegreeIdentifier
{
    [JsonPropertyName("degree_id")]
    [Key("degree_id")]
    public int DegreeId { get; set; }
}