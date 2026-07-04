using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/entry</c> — pay the entry cost and start a Colosseum bracket
/// attempt. Maps to <c>Wizard/ColosseumEntryTask.ColosseumEntryTaskParam</c>.
/// </summary>
[MessagePackObject(keyAsPropertyName: false)]
public class ArenaColosseumEntryRequest : BaseRequest
{
    /// <summary>Currency selector — eARENA_PAY enum. 1=Crystal, 3=Ticket, 4=Rupy, 5=Free.</summary>
    [JsonPropertyName("consume_item_type")] [Key("consume_item_type")]
    public int ConsumeItemType { get; set; }

    /// <summary>Client-echoed round id from the most recent <c>/get_fee_info</c> or <c>/top</c>.
    /// Server rejects if it disagrees with the current server-decided round.</summary>
    [JsonPropertyName("now_round_id")] [Key("now_round_id")]
    public int NowRoundId { get; set; }
}
