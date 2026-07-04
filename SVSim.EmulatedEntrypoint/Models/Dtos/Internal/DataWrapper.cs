using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Internal;

/// <summary>
/// Wraps responses in the format the official game client expects, with a header section for additional data. Not for manual endpoint use, this wrapping is done automatically in a middleware.
/// </summary>
[MessagePackObject]
public class DataWrapper
{
    /// <summary>
    /// Wire-shape projection of the response envelope headers. The middleware builds a
    /// strongly-typed <see cref="DataHeaders"/> POCO and runs it through the same STJ +
    /// <c>ConvertJsonTreeToPlainObject</c> pipeline that the controller's response goes
    /// through, yielding this dict with absent keys for null-valued optional fields.
    /// Typed as <see cref="Dictionary{TKey,TValue}"/> (not <see cref="object"/>) because
    /// the projected shape is fully known — only the per-key value type varies. Direct
    /// assignment of the typed POCO would let MessagePack's contractless resolver emit
    /// <c>"key":null</c> for nullables, which the client treats as "key present" via
    /// <c>Keys.Contains</c> (see <c>NetworkTask.isResourceVersionUp</c> for the
    /// load-bearing case).
    /// </summary>
    [JsonPropertyName("data_headers")]
    [Key("data_headers")]
    public Dictionary<string, object?> DataHeaders { get; set; } = new();

    /// <summary>
    /// The response data from the endpoint.
    /// </summary>
    [JsonPropertyName("data")]
    [Key("data")] 
    public object Data { get; set; } = new();
}