using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class ReceiveApplyInfoResponse
{
    [JsonPropertyName("receive_applies")][Key("receive_applies")] public List<FriendApplyEntryDto> ReceiveApplies { get; set; } = new();
    [JsonPropertyName("approve_apply_count")][Key("approve_apply_count")] public int ApproveApplyCount { get; set; }
}
