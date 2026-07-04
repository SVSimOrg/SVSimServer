using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BuildDeck;

/// <summary>
/// /build_deck/info request body. <c>add_series_id == 0</c> means "return all"; non-zero filters
/// to the single matching series (used by the client to re-fetch after a purchase).
/// </summary>
[MessagePackObject]
public class BuildDeckInfoRequest : BaseRequest
{
    [JsonPropertyName("add_series_id")]
    [Key("add_series_id")]
    public int AddSeriesId { get; set; }
}
