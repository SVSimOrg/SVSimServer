using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

[MessagePackObject]
public sealed class MonthlyRankingResponseDto
{
    [JsonPropertyName("period"), Key("period")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public PeriodEntryDto Period { get; set; } = new();

    [JsonPropertyName("ranking"), Key("ranking")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<RankingEntryDto> Ranking { get; set; } = new();
}
