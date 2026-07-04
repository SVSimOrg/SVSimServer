using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class FriendInfoResponse
{
    [JsonPropertyName("friends")][Key("friends")] public List<FriendEntryDto> Friends { get; set; } = new();
    [JsonPropertyName("friend_count")][Key("friend_count")] public int FriendCount { get; set; }
    [JsonPropertyName("friend_max_count")][Key("friend_max_count")] public int FriendMaxCount { get; set; }
}
