using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Pack;

/// <summary>
/// Inbound /pack/exchange_gacha_point body. See
/// <see cref="GetGachaPointRewardsRequest"/> for the odds_gacha_id vs parent_gacha_id split —
/// same pattern here: the server consumes <c>odds_gacha_id</c> for the lookup.
/// </summary>
[MessagePackObject]
public class ExchangeGachaPointRequest : BaseRequest
{
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public long CardId { get; set; }

    [JsonPropertyName("parent_gacha_id")]
    [Key("parent_gacha_id")]
    public int ParentGachaId { get; set; }

    [JsonPropertyName("odds_gacha_id")]
    [Key("odds_gacha_id")]
    public int OddsGachaId { get; set; }
}
