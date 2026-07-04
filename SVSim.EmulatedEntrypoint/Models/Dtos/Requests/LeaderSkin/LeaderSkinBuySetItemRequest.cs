using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.LeaderSkin;

/// <summary>
/// /leader_skin/buy_set_item — claim the series-completion bonus once every skin in the series
/// is owned. <c>sales_type</c> field exists on the client's param class but is never set; server
/// ignores it.
/// </summary>
[MessagePackObject]
public class LeaderSkinBuySetItemRequest : BaseRequest
{
    [JsonPropertyName("series_id")]
    [Key("series_id")]
    public int SeriesId { get; set; }
}
