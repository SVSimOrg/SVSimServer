using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

/// <summary>
/// Search result. When no match, <see cref="UserInfo"/> serializes as <c>{}</c>
/// (an empty JSON object). When matched, it's a populated <see cref="FriendEntryDto"/>.
/// </summary>
[MessagePackObject]
public sealed class SearchUserResponse
{
    [JsonPropertyName("user_info")][Key("user_info")]
    public object UserInfo { get; set; } = new();
}
