using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

[MessagePackObject]
public sealed class PeriodListResponseDto
{
    // All required per spec; emit empty list, never null.
    [JsonPropertyName("rank_match"), Key("rank_match")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<PeriodEntryDto> RankMatch { get; set; } = new();

    [JsonPropertyName("master_point"), Key("master_point")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<MasterPointPeriodEntryDto> MasterPoint { get; set; } = new();

    [JsonPropertyName("two_pick"), Key("two_pick")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<TwoPickPeriodEntryDto> TwoPick { get; set; } = new();

    [JsonPropertyName("sealed"), Key("sealed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<PeriodEntryDto> Sealed { get; set; } = new();

    [JsonPropertyName("crossover_rank_match"), Key("crossover_rank_match")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<PeriodEntryDto> CrossoverRankMatch { get; set; } = new();

    [JsonPropertyName("crossover_master_point"), Key("crossover_master_point")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<PeriodEntryDto> CrossoverMasterPoint { get; set; } = new();
}
