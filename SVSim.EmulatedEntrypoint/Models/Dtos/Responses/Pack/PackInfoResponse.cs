using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Pack;

[MessagePackObject]
public class PackInfoResponse
{
    [JsonPropertyName("pack_config_list")]
    [Key("pack_config_list")]
    public List<PackConfigDto> PackConfigList { get; set; } = new();
}
