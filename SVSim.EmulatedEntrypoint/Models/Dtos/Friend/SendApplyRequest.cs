using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class SendApplyRequest
{
    [JsonPropertyName("friend_id")][Key("friend_id")]
    public int FriendId { get; set; }
}
