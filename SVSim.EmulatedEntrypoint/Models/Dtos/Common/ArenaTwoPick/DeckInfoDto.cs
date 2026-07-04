using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;

[MessagePackObject]
public class DeckInfoDto
{
    [JsonPropertyName("two_pick_entry_id")] [JsonConverter(typeof(StringifiedLongConverter))] [Key("two_pick_entry_id")]
    public long TwoPickEntryId { get; set; }

    [JsonPropertyName("class_id")] [JsonConverter(typeof(StringifiedIntConverter))] [Key("class_id")]
    public int ClassId { get; set; }

    /// <summary>Native bool on the wire (NOT stringified — matches capture).</summary>
    [JsonPropertyName("is_select_completed")] [Key("is_select_completed")]
    public bool IsSelectCompleted { get; set; }

    /// <summary>Native long[] (raw ints in capture). Parser does .ToInt(); emitting as numbers matches prod.</summary>
    [JsonPropertyName("selected_card_ids")] [Key("selected_card_ids")]
    public List<long> SelectedCardIds { get; set; } = new();

    [JsonPropertyName("select_turn")] [Key("select_turn")]
    public int SelectTurn { get; set; }
}
