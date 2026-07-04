using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

/// <summary>
/// Inner reward block. STRING-typed on wire (capture confirms reward_type/reward_detail_id/
/// reward_number all serialize as JSON strings here, unlike UserMission where they're int).
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class BPMonthlyMissionRewardInfoDto
{
    [Key(0)][JsonPropertyName("reward_type")] public string RewardType { get; set; } = "";
    [Key(1)][JsonPropertyName("reward_detail_id")] public string RewardDetailId { get; set; } = "";
    [Key(2)][JsonPropertyName("reward_number")] public string RewardNumber { get; set; } = "";
}

/// <summary>
/// One BP monthly mission. reward_info is OPTIONAL — capture shows "Play 5 Challenge matches"
/// has no reward_info block (only BP points). Global WhenWritingNull policy omits when null.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class BPMonthlyMissionDto
{
    [Key(0)][JsonPropertyName("name")] public string Name { get; set; } = "";
    [Key(1)][JsonPropertyName("is_cleared")] public bool IsCleared { get; set; }
    [Key(2)][JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [Key(3)][JsonPropertyName("done_number")] public int DoneNumber { get; set; }
    [Key(4)][JsonPropertyName("battle_pass_point")] public int BattlePassPoint { get; set; }
    [Key(5)][JsonPropertyName("reward_info")] public BPMonthlyMissionRewardInfoDto? RewardInfo { get; set; }
}
