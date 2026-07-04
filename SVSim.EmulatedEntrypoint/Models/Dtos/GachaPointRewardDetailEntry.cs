using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One entry inside <c>gacha_point_rewards[i].reward_list</c>. Different shape from the
/// post-state-totals <see cref="RewardListEntry"/> used by /pack/open: this is a catalog
/// declaration ("here's what you'd get if you exchanged"), not a viewer-state assignment.
/// Wire keys verified against prod capture data_dumps/captures/traffic_prod_tradeables_capture.ndjson.
/// </summary>
[MessagePackObject]
public class GachaPointRewardDetailEntry
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_number")]
    [Key("reward_number")]
    public int RewardNumber { get; set; } = 1;
}
