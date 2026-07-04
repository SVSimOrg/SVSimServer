using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

/// <summary>
/// /battle_pass/buy request (Wizard/BattlePassBuyTask.cs:8-13). Inherits viewer_id, steam_id,
/// steam_session_ticket from BaseRequest. Per memory feedback_msgpack_request_dtos, the
/// [MessagePackObject] + [Key] attrs are required even though integration tests post JSON.
/// </summary>
[MessagePackObject]
public class BattlePassBuyRequest : BaseRequest
{
    [JsonPropertyName("season_id")]
    [Key("season_id")]
    public int SeasonId { get; set; }

    [JsonPropertyName("id")]
    [Key("id")]
    public int Id { get; set; }
}
