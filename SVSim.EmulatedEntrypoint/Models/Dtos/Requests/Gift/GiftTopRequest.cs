using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Gift;

[MessagePackObject]
public class GiftTopRequest : BaseRequest
{
    [JsonPropertyName("page")]
    [Key("page")]
    public int Page { get; set; }
}
