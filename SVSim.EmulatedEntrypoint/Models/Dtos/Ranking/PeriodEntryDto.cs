using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

/// <summary>
/// Base period-entry shape used by /ranking/get_viewable_ranking_period_list's
/// rank_match[] and crossover_* arrays. Master-point and two-pick variants
/// add fields — see <see cref="MasterPointPeriodEntryDto"/> and
/// <see cref="TwoPickPeriodEntryDto"/>.
/// </summary>
[MessagePackObject]
public class PeriodEntryDto
{
    [JsonPropertyName("id"), Key("id")]
    public string Id { get; set; } = "0";

    [JsonPropertyName("period_num"), Key("period_num")]
    public string PeriodNum { get; set; } = "0";

    [JsonPropertyName("begin_time"), Key("begin_time")]
    public string BeginTime { get; set; } = "";

    [JsonPropertyName("end_time"), Key("end_time")]
    public string EndTime { get; set; } = "";
}
