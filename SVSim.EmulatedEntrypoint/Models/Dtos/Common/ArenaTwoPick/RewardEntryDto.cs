using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

/// <summary>
/// One entry on the TK2 reward_list / rewards wire arrays. Matches the entry-response capture
/// (reward_type int, reward_id int, reward_num string).
/// </summary>
[MessagePackObject]
public class RewardEntryDto
{
    [JsonPropertyName("reward_type")] [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_id")] [Key("reward_id")]
    public long RewardId { get; set; }

    [JsonPropertyName("reward_num")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("reward_num")]
    public int RewardNum { get; set; }
}
