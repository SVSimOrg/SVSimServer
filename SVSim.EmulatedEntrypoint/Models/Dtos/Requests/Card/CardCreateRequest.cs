using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Card;

/// <summary>
/// POST /card/create. The single payload field is a JSON-encoded STRING (double-encoded —
/// see docs/api-spec/endpoints/post-login/card-create.md). Inner object maps
/// cardId → "&lt;num_to_create&gt;,&lt;client_possession_snapshot&gt;". Both inner values
/// are strings. Same wire format as /card/destruct; CardController parses both with the
/// shared TryParseCardCountDict helper.
/// </summary>
[MessagePackObject]
public class CardCreateRequest : BaseRequest
{
    [JsonPropertyName("card_id_number_array")]
    [Key("card_id_number_array")]
    public string CardIdNumberArray { get; set; } = string.Empty;
}
