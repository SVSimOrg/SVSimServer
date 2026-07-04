using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// A card in a user's collection and the number possessed.
/// </summary>
[MessagePackObject]
public class UserCard : CardIdentifier
{
    /// <summary>
    /// The number of the specified card the user has.
    /// </summary>
    [JsonPropertyName("number")]
    [Key("number")]
    public int Count { get; set; }
    
    /// <summary>
    /// Whether the card is protected from dusting.
    /// </summary>
    [JsonPropertyName("is_protected")]
    [Key("is_protected")]
    public int IsProtected { get; set; }

    public UserCard(OwnedCardEntry card)
    {
        this.CardId = card.Card.Id;
        this.Count = card.Count;
        this.IsProtected = card.IsProtected ? 1 : 0;
    }

    public UserCard()
    {
    }
}