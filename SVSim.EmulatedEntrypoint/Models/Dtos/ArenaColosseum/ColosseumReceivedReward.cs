using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Per-entry shape for the <c>rewards</c> array on <c>/finish</c> + <c>/retire</c>. Richer
/// than <c>RewardEntryDto</c> (which is the <c>reward_list</c> shape) — carries a display
/// name. Distinct from the wallet-delta block: client renders <c>rewards</c> in the
/// post-bracket popup while <c>reward_list</c> drives the silent wallet update.
/// </summary>
[MessagePackObject]
public sealed class ColosseumReceivedReward
{
    [JsonPropertyName("reward_number")] [Key("reward_number")]
    public int RewardNumber { get; set; }

    [JsonPropertyName("reward_type")] [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_detail_id")] [Key("reward_detail_id")]
    public long RewardDetailId { get; set; }

    [JsonPropertyName("name")] [Key("name")]
    public string Name { get; set; } = "";
}
