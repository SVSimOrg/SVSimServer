using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Gift;

[MessagePackObject]
public class GiftReceiveRequest : BaseRequest
{
    [JsonPropertyName("present_id_array")]
    [Key("present_id_array")]
    public List<string> PresentIdArray { get; set; } = new();

    [JsonPropertyName("state")]
    [Key("state")]
    public int State { get; set; }
}
