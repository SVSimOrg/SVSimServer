using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

[MessagePackObject]
public class EntryInfoDto
{
    [JsonPropertyName("id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("id")]
    public long Id { get; set; }

    [JsonPropertyName("viewer_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("viewer_id")]
    public long ViewerId { get; set; }

    [JsonPropertyName("reward_schedule_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("reward_schedule_id")]
    public int RewardScheduleId { get; set; }

    [JsonPropertyName("challenge_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("challenge_id")]
    public int ChallengeId { get; set; }

    [JsonPropertyName("max_battle_count")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("max_battle_count")]
    public int MaxBattleCount { get; set; }

    [JsonPropertyName("leader_skin_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("leader_skin_id")]
    public long LeaderSkinId { get; set; }

    [JsonPropertyName("is_retire")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("is_retire")]
    public int IsRetire { get; set; }
}
