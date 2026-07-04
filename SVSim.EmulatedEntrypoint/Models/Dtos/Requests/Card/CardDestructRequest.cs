using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Card;

/// <summary>
/// POST /card/destruct. The single payload field is a JSON-encoded STRING (double-encoded
/// — see docs/api-spec/endpoints/post-login/card-destruct.md). The inner object maps
/// cardId → "&lt;num_to_destruct&gt;,&lt;client_possession_snapshot&gt;". Both inner values
/// are strings. This DTO keeps it as a single string; parsing happens in CardController.
/// </summary>
[MessagePackObject]
public class CardDestructRequest : BaseRequest
{
    [JsonPropertyName("card_id_number_array")]
    [Key("card_id_number_array")]
    public string CardIdNumberArray { get; set; } = string.Empty;
}
