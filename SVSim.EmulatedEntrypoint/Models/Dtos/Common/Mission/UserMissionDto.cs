using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

/// <summary>
/// Wire shape of UserMission (per MissionInfoDetail.cs:75-95). lot_type and battle_pass_point
/// are STRING-typed on wire (client uses .ToInt() but emits as string in capture). All other
/// scalar fields are int. end_time omitted when null per UserMission.Parse() optional read.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class UserMissionDto
{
    [Key(0)][JsonPropertyName("id")] public long Id { get; set; }
    [Key(1)][JsonPropertyName("mission_id")] public int MissionId { get; set; }
    [Key(2)][JsonPropertyName("total_count")] public int TotalCount { get; set; }
    [Key(3)][JsonPropertyName("mission_status")] public int MissionStatus { get; set; }
    [Key(4)][JsonPropertyName("display_order")] public int DisplayOrder { get; set; }
    [Key(5)][JsonPropertyName("mission_name")] public string MissionName { get; set; } = "";
    [Key(6)][JsonPropertyName("lot_type")] public string LotType { get; set; } = "";
    [Key(7)][JsonPropertyName("battle_pass_point")] public string BattlePassPoint { get; set; } = "";
    [Key(8)][JsonPropertyName("require_number")] public int RequireNumber { get; set; }
    [Key(9)][JsonPropertyName("reward_type")] public int RewardType { get; set; }
    [Key(10)][JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [Key(11)][JsonPropertyName("reward_number")] public int RewardNumber { get; set; }
    [Key(12)][JsonPropertyName("default_flag")] public bool DefaultFlag { get; set; }
    [Key(13)][JsonPropertyName("start_time")] public long StartTime { get; set; }
    [Key(14)][JsonPropertyName("end_time")] public long? EndTime { get; set; }
}
