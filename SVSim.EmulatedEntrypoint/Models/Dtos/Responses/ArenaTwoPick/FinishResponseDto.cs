using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

[MessagePackObject]
public class FinishResponseDto
{
    /// <summary>
    /// Per-grant deltas — drives the "+N received" popup. Parsed by the client via
    /// <c>ReceivedReward(JsonData)</c> (Shadowverse_Code/ReceivedReward.cs:25) which expects
    /// the {reward_type, reward_detail_id, item_type, reward_count?, is_usable} shape — distinct
    /// from the <c>RewardEntryDto</c> shape used by <see cref="RewardList"/>.
    /// </summary>
    [JsonPropertyName("rewards")] [Key("rewards")]
    public List<TwoPickRewardReceivedDto> Rewards { get; set; } = new();

    /// <summary>Post-state totals — drives PlayerStaticData.UpdateHaveUserGoodsNumByJsonData.</summary>
    [JsonPropertyName("reward_list")] [Key("reward_list")]
    public List<RewardEntryDto> RewardList { get; set; } = new();
}

/// <summary>
/// Wire shape parsed by Shadowverse_Code/ReceivedReward.cs ctor. Used in the
/// <c>rewards</c> arrays of /arena_two_pick/{retire,finish}.
/// </summary>
[MessagePackObject]
public class TwoPickRewardReceivedDto
{
    [JsonPropertyName("reward_type")] [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")] [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("reward_count")] [Key("reward_count")]
    public int RewardCount { get; set; }

    /// <summary>
    /// Item-master <c>item_type</c> enum (1=challenge ticket, 2=card-pack ticket, …) for Item-typed
    /// rewards. 0 for currencies (Crystal/Rupy/RedEther) — the client only reads this for Items.
    /// </summary>
    [JsonPropertyName("item_type")] [Key("item_type")]
    public int ItemType { get; set; }

    [JsonPropertyName("is_usable")] [Key("is_usable")]
    public bool IsUsable { get; set; } = true;
}
