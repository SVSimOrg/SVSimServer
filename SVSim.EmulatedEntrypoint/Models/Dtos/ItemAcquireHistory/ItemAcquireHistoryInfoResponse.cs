using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ItemAcquireHistory;

[MessagePackObject]
public sealed class ItemAcquireHistoryInfoResponse
{
    [JsonPropertyName("histories")]
    [Key("histories")]
    public List<ItemAcquireHistoryEntryDto> Histories { get; set; } = new();
}
