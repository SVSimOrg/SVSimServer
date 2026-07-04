using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// Shared empty response. Used for endpoints whose spec mock is `"data": {}`
/// (set_deck_redis, update_order, delete_deck_list, etc.). Includes a sentinel
/// nullable field so MessagePack-CSharp emits a string-keyed empty map cleanly.
/// </summary>
[MessagePackObject]
public class EmptyResponse
{
    [JsonPropertyName("_")]
    [Key("_")] public object? Reserved { get; set; }
}
