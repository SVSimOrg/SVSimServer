using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Mission;

[MessagePackObject]
public class MissionRetireRequest : BaseRequest
{
    [Key("id")]
    [JsonPropertyName("id")]
    public long Id { get; set; }
}
