using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Card;

/// <summary>
/// /card/create response data. reward_list entries are POST-STATE TOTALS (the client's
/// PlayerStaticData.UpdateHaveUserGoodsNumByJsonData does direct assignment). One
/// RewardType=1 RedEther entry plus one RewardType=5 Card entry per crafted cardId,
/// plus cascade entries for any CardCosmeticReward rows attached to the crafted cards.
/// </summary>
[MessagePackObject]
public class CardCreateResponse
{
    [JsonPropertyName("reward_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
