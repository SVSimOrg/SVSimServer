using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Wire shape for a single curated deck on <c>/get_{hof|windfall|avatar}_deck_list</c>.
/// The list response is a BARE ARRAY at the <c>data</c> level per spec — client iterates
/// directly without a wrapper object. Sleeve/skin are optional; client falls back to
/// defaults when absent.
/// </summary>
[MessagePackObject]
public sealed class ColosseumCuratedDeckEntry
{
    [JsonPropertyName("deck_id")] [Key("deck_id")]
    public int DeckId { get; set; }

    [JsonPropertyName("class_id")] [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("card_list")] [Key("card_list")]
    public List<long> CardList { get; set; } = new();

    [JsonPropertyName("sleeve_id")] [Key("sleeve_id")]
    public long? SleeveId { get; set; }

    [JsonPropertyName("skin_id")] [Key("skin_id")]
    public long? SkinId { get; set; }

    [JsonPropertyName("deck_name")] [Key("deck_name")]
    public string? DeckName { get; set; }
}
