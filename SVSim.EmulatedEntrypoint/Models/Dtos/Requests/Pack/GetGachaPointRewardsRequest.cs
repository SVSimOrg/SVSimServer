using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Pack;

/// <summary>
/// Inbound /pack/get_gacha_point_rewards body.
///
/// The two ids DIFFER for seasonal packs (e.g. UCL):
///   odds_gacha_id   = the seasonal "current" pack id (matches /pack/info parent_gacha_id),
///                     and is where the GachaPointConfig + gacha-point balance live.
///   parent_gacha_id = the base/family pack id (matches /pack/info base_pack_id).
/// Verified against traffic_prod_all_gacha_exchange.ndjson — every captured request shows
/// the pair as (16xxx, 10xxx). Server consumes <c>odds_gacha_id</c> for the lookup.
/// </summary>
[MessagePackObject]
public class GetGachaPointRewardsRequest : BaseRequest
{
    [JsonPropertyName("odds_gacha_id")]
    [Key("odds_gacha_id")]
    public int OddsGachaId { get; set; }

    [JsonPropertyName("parent_gacha_id")]
    [Key("parent_gacha_id")]
    public int ParentGachaId { get; set; }
}
