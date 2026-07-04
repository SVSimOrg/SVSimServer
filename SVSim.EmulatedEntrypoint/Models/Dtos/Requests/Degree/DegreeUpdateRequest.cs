using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Degree;

[MessagePackObject]
public class DegreeUpdateRequest : BaseRequest
{
    [JsonPropertyName("degree_id")]
    [Key("degree_id")]
    public int DegreeId { get; set; }
}
