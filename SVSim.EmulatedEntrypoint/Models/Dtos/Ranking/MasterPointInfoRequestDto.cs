using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

[MessagePackObject]
public sealed class MasterPointInfoRequestDto : BaseRequest
{
    [JsonPropertyName("period_id"), Key("period_id")]
    public int PeriodId { get; set; }
}
