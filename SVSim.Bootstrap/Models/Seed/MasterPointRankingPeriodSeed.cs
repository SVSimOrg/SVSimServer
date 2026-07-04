using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class MasterPointRankingPeriodSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("period_num")] public int PeriodNum { get; set; }
    [JsonPropertyName("necessary_score")] public long NecessaryScore { get; set; }
    [JsonPropertyName("begin_time")] public string BeginTime { get; set; } = "";
    [JsonPropertyName("end_time")] public string EndTime { get; set; } = "";
}
