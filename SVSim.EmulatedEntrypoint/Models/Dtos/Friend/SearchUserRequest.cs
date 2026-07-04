using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

[MessagePackObject]
public sealed class SearchUserRequest
{
    [JsonPropertyName("search_viewer_id")][Key("search_viewer_id")]
    public int SearchViewerId { get; set; }
}
