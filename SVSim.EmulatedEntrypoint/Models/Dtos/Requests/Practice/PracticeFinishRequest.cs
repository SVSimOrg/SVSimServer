using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Practice;

[MessagePackObject]
public class PracticeFinishRequest : BaseRequest
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("is_win")]
    [Key("is_win")] public int IsWin { get; set; }
    [JsonPropertyName("evolve_count")]
    [Key("evolve_count")] public int EvolveCount { get; set; }
    [JsonPropertyName("total_turn")]
    [Key("total_turn")] public int TotalTurn { get; set; }
    [JsonPropertyName("enemy_class_id")]
    [Key("enemy_class_id")] public int EnemyClassId { get; set; }
    [JsonPropertyName("difficulty")]
    [Key("difficulty")] public int Difficulty { get; set; }
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }
    [JsonPropertyName("class_id")]
    [Key("class_id")] public int ClassId { get; set; }

    [JsonPropertyName("mission")]
    [Key("mission")] public Dictionary<string, int>? Mission { get; set; }

    /// <summary>
    /// JSON blob 窶・`recovery_single.json` serialized to string. Always present; not validated
    /// server-side (audit-flagged as out of scope for v1).
    /// </summary>
    [JsonPropertyName("recovery_data")]
    [Key("recovery_data")] public string? RecoveryData { get; set; }

    /// <summary>
    /// Misspelled the same way in every solo finish endpoint 窶・preserved on the wire.
    /// See spec note on practice-finish.md.
    /// </summary>
    [JsonPropertyName("prosessing_time_data")]
    [Key("prosessing_time_data")] public List<string>? ProsessingTimeData { get; set; }
}
