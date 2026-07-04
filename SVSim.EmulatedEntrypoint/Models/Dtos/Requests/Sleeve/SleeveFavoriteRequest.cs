using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Sleeve;

[MessagePackObject]
public class SleeveFavoriteRequest : BaseRequest
{
    // Spec note: ids are STRINGS on the wire, not ints. Don't change to long[].
    [JsonPropertyName("favorite_add")]
    [Key("favorite_add")]
    public List<string> FavoriteAdd { get; set; } = new();

    [JsonPropertyName("favorite_remove")]
    [Key("favorite_remove")]
    public List<string> FavoriteRemove { get; set; } = new();
}
