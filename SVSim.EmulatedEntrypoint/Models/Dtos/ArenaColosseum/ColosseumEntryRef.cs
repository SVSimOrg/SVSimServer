using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>Wire <c>entry_info</c> object on <c>/top</c> and <c>/entry</c>. Reused across endpoints.</summary>
[MessagePackObject]
public class ColosseumEntryRef
{
    [JsonPropertyName("id")] [Key("id")]
    public long Id { get; set; }

    /// <summary>Used by <c>/entry</c> only — Format enum integer. Top emits via colosseum_info.</summary>
    [JsonPropertyName("deck_format")] [Key("deck_format")]
    public int? DeckFormat { get; set; }
}
