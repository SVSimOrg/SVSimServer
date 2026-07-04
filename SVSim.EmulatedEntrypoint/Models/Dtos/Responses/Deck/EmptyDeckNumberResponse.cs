using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;

[MessagePackObject]
public class EmptyDeckNumberResponse
{
    /// <summary>The next free deck slot number. 0 indicates "no slots available".</summary>
    [JsonPropertyName("empty_deck_num")]
    [Key("empty_deck_num")] public int EmptyDeckNum { get; set; }
}
