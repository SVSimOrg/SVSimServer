using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Card;

/// <summary>
/// /card/destruct response data. reward_list entries are POST-STATE TOTALS (the client's
/// PlayerStaticData.UpdateHaveUserGoodsNumByJsonData does direct assignment) — one
/// RewardType=1 RedEther entry plus one RewardType=5 Card entry per destructed cardId.
/// </summary>
[MessagePackObject]
public class CardDestructResponse
{
    [JsonPropertyName("reward_list")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
