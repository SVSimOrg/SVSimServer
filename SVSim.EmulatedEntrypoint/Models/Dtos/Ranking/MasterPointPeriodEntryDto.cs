using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

[MessagePackObject]
public sealed class MasterPointPeriodEntryDto : PeriodEntryDto
{
    [JsonPropertyName("necessary_score"), Key("necessary_score")]
    public string NecessaryScore { get; set; } = "0";
}
