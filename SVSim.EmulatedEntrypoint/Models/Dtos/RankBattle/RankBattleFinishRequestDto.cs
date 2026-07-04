using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.RankBattle;

/// <summary>
/// Standard BattleFinishParam shape — see docs/api-spec/common/types.ts.md and
/// docs/api-spec/endpoints/post-login/rank-battle/finish.md. Future: promote to
/// a shared common DTO when a second finish endpoint reuses this.
///
/// Inherits viewer_id / steam_id / steam_session_ticket from <see cref="BaseRequest"/>
/// so the auth fields survive the translation-middleware round-trip.
/// </summary>
[MessagePackObject]
public sealed class RankBattleFinishRequestDto : BaseRequest
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

    // RankBattleFinishTask extends BattleFinishParam with SDTRB.
    [JsonPropertyName("sdtrb")]
    [Key("sdtrb")]
    public int Sdtrb { get; set; }
}
