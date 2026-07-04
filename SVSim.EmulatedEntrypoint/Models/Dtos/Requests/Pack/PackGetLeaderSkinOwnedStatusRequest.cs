using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Pack;

[MessagePackObject]
public class PackGetLeaderSkinOwnedStatusRequest : BaseRequest
{
    [JsonPropertyName("parent_gacha_id")]
    [Key("parent_gacha_id")]
    public int ParentGachaId { get; set; }
}
