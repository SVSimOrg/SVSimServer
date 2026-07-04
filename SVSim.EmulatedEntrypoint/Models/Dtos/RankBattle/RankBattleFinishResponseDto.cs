using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.RankBattle;

/// <summary>
/// Stubbed Phase-3 rank-finish payload. Per RankBattleFinishTask.cs:57-63, the client
/// uses GetValueOrDefault(key, 0) for the seven primary scalars and Keys.Contains
/// for everything else — emitting zeros is safe. All-optional fields beyond these
/// eleven are deliberately omitted (no mission/treasure-box/battle-pass evaluation
/// in Phase 3 scope).
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public sealed class RankBattleFinishResponseDto
{
    [JsonPropertyName("rank")]
    [Key("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("after_battle_point")]
    [Key("after_battle_point")]
    public int AfterBattlePoint { get; set; }

    [JsonPropertyName("after_master_point")]
    [Key("after_master_point")]
    public int AfterMasterPoint { get; set; }

    [JsonPropertyName("battle_point")]
    [Key("battle_point")]
    public int BattlePoint { get; set; }

    [JsonPropertyName("master_point")]
    [Key("master_point")]
    public int MasterPoint { get; set; }

    [JsonPropertyName("successive_win_number")]
    [Key("successive_win_number")]
    public int SuccessiveWinNumber { get; set; }

    [JsonPropertyName("successive_win_bonus")]
    [Key("successive_win_bonus")]
    public int SuccessiveWinBonus { get; set; }

    [JsonPropertyName("battle_result")]
    [Key("battle_result")]
    public int BattleResult { get; set; }

    [JsonPropertyName("get_class_experience")]
    [Key("get_class_experience")]
    public int GetClassExperience { get; set; }

    [JsonPropertyName("class_experience")]
    [Key("class_experience")]
    public int ClassExperience { get; set; }

    [JsonPropertyName("class_level")]
    [Key("class_level")]
    public int ClassLevel { get; set; } = 1;
}
