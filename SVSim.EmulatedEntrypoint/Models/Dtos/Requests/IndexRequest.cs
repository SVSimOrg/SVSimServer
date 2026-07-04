using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class IndexRequest : BaseRequest
{
    [JsonPropertyName("carrier")]
    [Key("carrier")]
    public string Carrier { get; set; }

    [JsonPropertyName("card_master_hash")]
    [Key("card_master_hash")]
    public string CardMasterHash { get; set; }
}
