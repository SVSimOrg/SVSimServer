using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ImmutableData;

/// <summary>
/// Wire body for <c>POST /immutable_data/card_master</c>. Client populates from
/// <c>GetCardMasterTask.CardMasterTaskParam</c> (Wizard/GetCardMasterTask.cs:14). For Tier 1
/// the hash is bound only for completeness — the controller serves the static blob regardless.
/// </summary>
[MessagePackObject]
public class CardMasterRequest : BaseRequest
{
    [JsonPropertyName("card_master_hash")]
    [Key("card_master_hash")]
    public string CardMasterHash { get; set; } = "";
}
