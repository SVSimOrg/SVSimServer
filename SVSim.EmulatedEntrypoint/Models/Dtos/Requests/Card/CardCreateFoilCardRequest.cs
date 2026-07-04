using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Card;

/// <summary>
/// POST /card/create_foil_card — Seer's Globe conversion (Wizard/PremiumCardConversionTask.cs).
/// Consumes <see cref="CreateNumber"/> Orbs (Item id 1000) + <see cref="CreateNumber"/> copies of
/// <see cref="BaseCardId"/> to grant <see cref="CreateNumber"/> copies of the foil twin (base+1).
/// </summary>
[MessagePackObject]
public class CardCreateFoilCardRequest : BaseRequest
{
    [JsonPropertyName("base_card_id")]
    [Key("base_card_id")]
    public int BaseCardId { get; set; }

    /// <summary>
    /// Client's snapshot of how many normal copies it has at request time. Sanity-check only —
    /// the actual spend is <c>create_number</c> copies, not this value.
    /// </summary>
    [JsonPropertyName("base_card_number")]
    [Key("base_card_number")]
    public int BaseCardNumber { get; set; }

    /// <summary>Hardcoded to 1 by <c>PremiumCardConversionTask.SetParameter</c>.</summary>
    [JsonPropertyName("create_number")]
    [Key("create_number")]
    public int CreateNumber { get; set; }
}
