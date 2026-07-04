using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

[MessagePackObject]
public sealed class ClassWinInfoRequestDto : BaseRequest
{
    [JsonPropertyName("period_id"), Key("period_id")]
    public int PeriodId { get; set; }

    [JsonPropertyName("class_id"), Key("class_id")]
    public int ClassId { get; set; }
}
