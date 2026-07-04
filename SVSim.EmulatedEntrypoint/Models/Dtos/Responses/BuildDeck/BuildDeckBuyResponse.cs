using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BuildDeck;

/// <summary>
/// /build_deck/buy response. reward_list items use reward_id/reward_num (driven by
/// PlayerStaticData.UpdateHaveUserGoodsNumByJsonData with POST-STATE-TOTAL semantics);
/// series_rewards items use reward_detail_id/reward_number — different naming, intentional.
/// </summary>
[MessagePackObject]
public class BuildDeckBuyResponse
{
    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardListEntry> RewardList { get; set; } = new();

    [JsonPropertyName("series_rewards")]
    [Key("series_rewards")]
    public List<BuildDeckProductRewardDto> SeriesRewards { get; set; } = new();
}
