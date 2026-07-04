using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.LeaderSkin;

/// <summary>
/// /leader_skin/buy_set — purchase every skin in a series in one call (cheaper per-skin).
/// </summary>
[MessagePackObject]
public class LeaderSkinBuySetRequest : BaseRequest
{
    [JsonPropertyName("series_id")]
    [Key("series_id")]
    public int SeriesId { get; set; }

    [JsonPropertyName("sales_type")]
    [Key("sales_type")]
    public int SalesType { get; set; }

    [JsonPropertyName("item_id")]
    [Key("item_id")]
    public long? ItemId { get; set; }
}
