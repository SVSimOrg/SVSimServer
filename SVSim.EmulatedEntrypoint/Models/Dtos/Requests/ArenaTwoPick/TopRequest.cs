using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;

[MessagePackObject]
public class TopRequest : BaseRequest
{
    [JsonPropertyName("mode")] [Key("mode")] public int Mode { get; set; }
}
