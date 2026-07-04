using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class ConfigUpdateRequest : BaseRequest
{
    [JsonPropertyName("is_skip_gacha_effect")]
    [Key("is_skip_gacha_effect")]
    public int IsSkipGachaEffect { get; set; }
}
