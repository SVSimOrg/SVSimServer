using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Pack;

[MessagePackObject]
public class ExchangeGachaPointResponse
{
    /// <summary>
    /// POST-STATE TOTALS dispatched through PlayerStaticData.UpdateHaveUserGoodsNumByJsonData
    /// on the client (see project_wire_reward_list_post_state memory). The granted card,
    /// any cascading cosmetics, and the updated gacha-point balance entry all appear here.
    /// </summary>
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
