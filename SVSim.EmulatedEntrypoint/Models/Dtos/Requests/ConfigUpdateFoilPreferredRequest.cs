using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class ConfigUpdateFoilPreferredRequest : BaseRequest
{
    [JsonPropertyName("is_foil_preferred")]
    [Key("is_foil_preferred")]
    public int IsFoilPreferred { get; set; }
}
