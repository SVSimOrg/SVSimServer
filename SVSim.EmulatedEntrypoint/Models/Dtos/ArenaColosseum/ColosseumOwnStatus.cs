using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Per-viewer Colosseum state. All fields optional — when the viewer has no run, ALL fields
/// are null and the global <c>WhenWritingNull</c> policy renders this as <c>{}</c>, which
/// the client's <c>SetColosseumOwnStatus</c> short-circuits with <c>status.Count != 0</c>.
/// </summary>
[MessagePackObject]
public class ColosseumOwnStatus
{
    [JsonPropertyName("rest_entry_num")] [Key("rest_entry_num")]
    public int? RestEntryNum { get; set; }

    [JsonPropertyName("now_round_id")] [Key("now_round_id")]
    public int? NowRoundId { get; set; }

    [JsonPropertyName("next_round_id")] [Key("next_round_id")]
    public int? NextRoundId { get; set; }

    [JsonPropertyName("is_last_day")] [Key("is_last_day")]
    public bool? IsLastDay { get; set; }

    [JsonPropertyName("is_champion")] [Key("is_champion")]
    public bool? IsChampion { get; set; }

    /// <summary>Only present when <see cref="IsChampion"/> is true — client uses it to overwrite
    /// <c>ColosseumData.Name</c> for the champion screen.</summary>
    [JsonPropertyName("colosseum_name")] [Key("colosseum_name")]
    public string? ColosseumName { get; set; }
}
