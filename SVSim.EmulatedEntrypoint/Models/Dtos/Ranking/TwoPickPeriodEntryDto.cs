using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

[MessagePackObject]
public sealed class TwoPickPeriodEntryDto : PeriodEntryDto
{
    [JsonPropertyName("type"), Key("type")]
    public string Type { get; set; } = "2";

    [JsonPropertyName("over_460"), Key("over_460")]
    public string Over460 { get; set; } = "1";
}
