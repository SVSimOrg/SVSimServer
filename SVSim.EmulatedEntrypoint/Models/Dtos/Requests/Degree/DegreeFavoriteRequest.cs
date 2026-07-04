using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Degree;

[MessagePackObject]
public class DegreeFavoriteRequest : BaseRequest
{
    // Spec note: ids are STRINGS on the wire, not ints. Same gotcha as /sleeve/favorite and /emblem/favorite.
    [JsonPropertyName("favorite_add")]
    [Key("favorite_add")]
    public List<string> FavoriteAdd { get; set; } = new();

    [JsonPropertyName("favorite_remove")]
    [Key("favorite_remove")]
    public List<string> FavoriteRemove { get; set; } = new();
}
