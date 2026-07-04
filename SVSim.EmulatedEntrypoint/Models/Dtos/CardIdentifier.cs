using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Identifies a card in the game system.
/// </summary>
[MessagePackObject]
public class CardIdentifier
{
    /// <summary>
    /// The identifier of the card.
    /// </summary>
    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public long CardId { get; set; }
}