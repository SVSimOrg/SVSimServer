using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class SpecialRotationSchedule
{
    [JsonPropertyName("gathering")]
    [Key("gathering")]
    public DateRange Gathering { get; set; } = new DateRange();
    [JsonPropertyName("free_battle")]
    [Key("free_battle")]
    public DateRange FreeBattle { get; set; } = new DateRange();
}