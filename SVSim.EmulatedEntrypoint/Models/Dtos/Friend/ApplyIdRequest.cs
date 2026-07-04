using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class ApplyIdRequest
{
    [JsonPropertyName("apply_id")][Key("apply_id")]
    public int ApplyId { get; set; }
}
