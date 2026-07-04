using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.DeckBuilder;

/// <summary>
/// Portal resolve-by-code request. Anonymous on the wire — does not extend
/// <see cref="BaseRequest"/>; see <see cref="GenerateDeckCodeRequest"/> for the rationale.
/// </summary>
[MessagePackObject]
public class GetDeckFromCodeRequest
{
    [JsonPropertyName("deck_code")]
    [Key("deck_code")]
    public string DeckCode { get; set; } = "";
}
