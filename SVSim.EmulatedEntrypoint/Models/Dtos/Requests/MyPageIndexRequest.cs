using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class MyPageIndexRequest : BaseRequest
{
    [JsonPropertyName("carrier")]
    [Key("carrier")]
    public string Carrier { get; set; } = string.Empty;
}
