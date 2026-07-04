using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Sleeve;

/// <summary>
/// /sleeve/buy response. <c>reward_list</c> items use reward_id/reward_num
/// (POST-STATE-TOTAL for currencies, grant id+count for cosmetics) — driven by
/// <c>PlayerStaticData.UpdateHaveUserGoodsNumByJsonData</c>. Mirrors the /pack/open +
/// /build_deck/buy reward_list semantics.
/// </summary>
[MessagePackObject]
public class SleeveBuyResponse
{
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();
}
