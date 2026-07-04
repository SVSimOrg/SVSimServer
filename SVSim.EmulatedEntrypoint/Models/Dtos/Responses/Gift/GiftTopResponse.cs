using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Gift;

[MessagePackObject]
public class GiftTopResponse
{
    [JsonPropertyName("present_list")]
    [Key("present_list")]
    public List<PresentDto> PresentList { get; set; } = new();

    [JsonPropertyName("present_history_list")]
    [Key("present_history_list")]
    public List<PresentDto> PresentHistoryList { get; set; } = new();

    [JsonPropertyName("limit_over_present_list")]
    [Key("limit_over_present_list")]
    public List<PresentDto> LimitOverPresentList { get; set; } = new();
}
