using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class ConfigUpdatePrizePreferredRequest : BaseRequest
{
    [JsonPropertyName("is_prize_preferred")]
    [Key("is_prize_preferred")]
    public int IsPrizePreferred { get; set; }
}
