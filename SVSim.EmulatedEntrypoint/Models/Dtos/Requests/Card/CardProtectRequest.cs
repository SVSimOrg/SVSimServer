using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Card;

/// <summary>
/// POST /card/protect. Toggles the protected-card flag for a single card. The client (see
/// Wizard/CardProtectTask.cs) sends is_protected as a real boolean — the 0|1 int inconsistency
/// noted in the spec lives on the /load/index response side and is out of scope here.
/// </summary>
[MessagePackObject]
public class CardProtectRequest : BaseRequest
{
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public long CardId { get; set; }

    [JsonPropertyName("is_protected")]
    [Key("is_protected")]
    public bool IsProtected { get; set; }
}
