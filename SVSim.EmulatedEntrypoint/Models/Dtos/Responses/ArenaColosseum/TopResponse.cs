using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/top</c> — lobby state for an in-progress run. When no season is
/// active <see cref="ColosseumInfo.IsColosseumPeriod"/> is <c>false</c> and most other
/// fields are absent (the client guards on that flag before reading anything else).
/// </summary>
[MessagePackObject]
public class TopResponse
{
    [JsonPropertyName("entry_info")] [Key("entry_info")]
    public ColosseumEntryRef EntryInfo { get; set; } = new();

    [JsonPropertyName("colosseum_info")] [Key("colosseum_info")]
    public ColosseumLobbyInfo ColosseumInfo { get; set; } = new();

    [JsonPropertyName("colosseum_status")] [Key("colosseum_status")]
    public ColosseumOwnStatus ColosseumStatus { get; set; } = new();

    [JsonPropertyName("now_round_id")] [Key("now_round_id")]
    public int NowRoundId { get; set; }

    [JsonPropertyName("user_deck")] [Key("user_deck")]
    public List<ColosseumUserDeck> UserDeck { get; set; } = new();

    [JsonPropertyName("max_battle_count")] [Key("max_battle_count")]
    public int MaxBattleCount { get; set; }

    [JsonPropertyName("is_finish")] [Key("is_finish")]
    public bool IsFinish { get; set; }

    [JsonPropertyName("final_round_eliminate_count")] [Key("final_round_eliminate_count")]
    public int FinalRoundEliminateCount { get; set; }

    [JsonPropertyName("end_time")] [Key("end_time")]
    public string EndTime { get; set; } = "";

    [JsonPropertyName("battle_results")] [Key("battle_results")]
    public ColosseumBattleResults BattleResults { get; set; } = new();

    [JsonPropertyName("breakthrough_number")] [Key("breakthrough_number")]
    public int? BreakthroughNumber { get; set; }

    [JsonPropertyName("box_grade_list")] [Key("box_grade_list")]
    public List<int>? BoxGradeList { get; set; }

    [JsonPropertyName("selected_chaos_id")] [Key("selected_chaos_id")]
    public int? SelectedChaosId { get; set; }

    /// <summary>ALWAYS emitted, even when 0. <c>WhenWritingNull</c> would strip this otherwise —
    /// see <c>project_wire_null_policy</c>: client does <c>jsonData["leader_skin_id"].ToInt()</c>
    /// unguarded, which throws a KeyNotFoundException if absent.</summary>
    [JsonPropertyName("leader_skin_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [Key("leader_skin_id")]
    public long LeaderSkinId { get; set; }
}
