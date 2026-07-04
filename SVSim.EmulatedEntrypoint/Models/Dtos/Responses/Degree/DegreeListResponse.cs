using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Degree;

[MessagePackObject]
public class DegreeListResponse
{
    [JsonPropertyName("user_degree_list")]
    [Key("user_degree_list")]
    public List<DegreeListEntry> UserDegreeList { get; set; } = new();
}
