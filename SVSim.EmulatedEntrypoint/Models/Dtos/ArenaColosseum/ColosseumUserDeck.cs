using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>Lightweight deck-info shape for <c>/top</c>'s <c>user_deck[0]</c>. The client's
/// <c>DeckData.Initialize</c> consumes the canonical deck shape; this is the minimum needed
/// to render the deck preview in Phase 1.</summary>
[MessagePackObject]
public class ColosseumUserDeck
{
    [JsonPropertyName("deck_id")] [Key("deck_id")]
    public long DeckId { get; set; }

    [JsonPropertyName("class_id")] [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("card_list")] [Key("card_list")]
    public List<long> CardList { get; set; } = new();

    [JsonPropertyName("sleeve_id")] [Key("sleeve_id")]
    public long? SleeveId { get; set; }

    [JsonPropertyName("skin_id")] [Key("skin_id")]
    public long? SkinId { get; set; }
}
