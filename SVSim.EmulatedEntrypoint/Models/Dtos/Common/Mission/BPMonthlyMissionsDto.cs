using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

/// <summary>
/// Outer block. Date strings use the capture's space-separated JST format
/// ("2026-05-01 02:00:00"). The whole block is omitted from /mission/info when no monthly
/// missions are seeded for the current month.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class BPMonthlyMissionsDto
{
    [Key(0)][JsonPropertyName("start_date")] public string StartDate { get; set; } = "";
    [Key(1)][JsonPropertyName("end_date")] public string EndDate { get; set; } = "";
    [Key(2)][JsonPropertyName("mission_list")] public List<BPMonthlyMissionDto> MissionList { get; set; } = new();
}
