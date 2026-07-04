using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.FreeBattle;

/// <summary>
/// BattleFinishParam shape for free-battle /finish. Mirrors the rank-battle variant
/// (Models/Dtos/RankBattle/RankBattleFinishRequestDto.cs) MINUS the <c>sdtrb</c> field
/// — RankBattleFinishTask extends BattleFinishParam to add sdtrb; FreeBattleFinishTask
/// does not (see Shadowverse_Code_2026-05-23/Wizard/FreeBattleFinishTask.cs).
///
/// Inherits viewer_id / steam_id / steam_session_ticket from <see cref="BaseRequest"/>
/// so the auth fields survive the translation-middleware round-trip.
/// </summary>
[MessagePackObject]
public sealed class FreeBattleFinishRequestDto : BaseRequest
{
    [JsonPropertyName("battle_result")]
    [Key("battle_result")]
    public int BattleResult { get; set; }

    [JsonPropertyName("is_retire")]
    [Key("is_retire")]
    public int IsRetire { get; set; }

    [JsonPropertyName("recovery_data")]
    [Key("recovery_data")]
    public string? RecoveryData { get; set; }

    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("total_turn")]
    [Key("total_turn")]
    public int TotalTurn { get; set; }

    [JsonPropertyName("evolve_count")]
    [Key("evolve_count")]
    public int EvolveCount { get; set; }

    [JsonPropertyName("enemy_evolve_count")]
    [Key("enemy_evolve_count")]
    public int EnemyEvolveCount { get; set; }
}
