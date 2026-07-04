using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class DateRange
{
    [JsonPropertyName("begin_time")]
    [Key("begin_time")]
    public DateTime BeginTime { get; set; }
    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public DateTime EndTime { get; set; }
}