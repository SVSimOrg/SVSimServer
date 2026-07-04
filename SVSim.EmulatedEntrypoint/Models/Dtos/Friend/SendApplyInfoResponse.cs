using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class SendApplyInfoResponse
{
    [JsonPropertyName("send_applies")][Key("send_applies")] public List<FriendApplyEntryDto> SendApplies { get; set; } = new();
    [JsonPropertyName("remaining_apply_count")][Key("remaining_apply_count")] public int RemainingApplyCount { get; set; }
    [JsonPropertyName("send_apply_max_count")][Key("send_apply_max_count")] public int SendApplyMaxCount { get; set; }
}
