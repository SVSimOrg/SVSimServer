using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Card;

/// <summary>
/// /card/create_foil_card response. reward_list entries are POST-STATE TOTALS — client's
/// <c>PlayerStaticData.UpdateHaveUserGoodsNumByJsonData</c> does direct assignment. Contains
/// the Item (Orb) post-balance, base-card post-count, and foil-card post-count.
/// </summary>
[MessagePackObject]
public class CardCreateFoilCardResponse
{
    [JsonPropertyName("reward_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
