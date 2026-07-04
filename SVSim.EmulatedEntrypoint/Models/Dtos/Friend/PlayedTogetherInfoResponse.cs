using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class PlayedTogetherInfoResponse
{
    [JsonPropertyName("histories")][Key("histories")] public List<PlayedTogetherEntryDto> Histories { get; set; } = new();
}
