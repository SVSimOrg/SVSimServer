using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;

/// <summary>
/// Single-deck-update response. Consumed by DeckListUtility.DeckUpdate(user_deck,
/// format, DeckAttributeType.CustomDeck). Shape is "one UserDeck wrapped under
/// `user_deck` key" 窶・same for update_name, update_sleeve, update_leader_skin,
/// update_random_leader_skin.
/// </summary>
[MessagePackObject]
public class SingleDeckResponse
{
    [JsonPropertyName("user_deck")]
    [Key("user_deck")] public UserDeck? UserDeck { get; set; }
}
