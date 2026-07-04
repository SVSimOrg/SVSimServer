using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.SpotCardExchange;

/// <summary>
/// /spot_card_exchange/exchange request — trade <see cref="ExchangePoint"/> spot points for
/// the card identified by <see cref="CardId"/>. The exchange_point field is the client's view
/// of the price (sanity-check it against the catalog server-side).
/// </summary>
[MessagePackObject]
public class SpotCardExchangeRequest : BaseRequest
{
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public int CardId { get; set; }

    [JsonPropertyName("exchange_point")]
    [Key("exchange_point")]
    public int ExchangePoint { get; set; }
}
