using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// <c>POST /colosseum_battle/finish</c> + <c>POST /colosseum_rank_battle/finish</c> request
/// body — the per-match finish, NOT the bracket-end <c>/arena_colosseum/finish</c>.
/// Standard <c>BattleFinishParam</c> shape inherited from <c>FinishTaskBase</c>.
/// </summary>
[MessagePackObject]
public sealed class ColosseumBattleFinishRequestDto : BaseRequest
{
    [JsonPropertyName("battle_result")] [Key("battle_result")]
    public int BattleResult { get; set; }

    [JsonPropertyName("is_retire")] [Key("is_retire")]
    public int IsRetire { get; set; }

    [JsonPropertyName("recovery_data")] [Key("recovery_data")]
    public string? RecoveryData { get; set; }

    [JsonPropertyName("class_id")] [Key("class_id")]
    public int ClassId { get; set; }

    [JsonPropertyName("total_turn")] [Key("total_turn")]
    public int TotalTurn { get; set; }

    [JsonPropertyName("evolve_count")] [Key("evolve_count")]
    public int EvolveCount { get; set; }

    [JsonPropertyName("enemy_evolve_count")] [Key("enemy_evolve_count")]
    public int EnemyEvolveCount { get; set; }
}

/// <summary>
/// <c>colosseum_battle/finish</c> response. Per battle-finish.md, the client maps this to
/// <c>ColosseumBattleFinishDetail</c> which is an empty <c>MatchFinishBase</c> subclass —
/// no Colosseum-specific fields beyond the shared rank-battle-finish superset. Phase 2 v1
/// emits the minimum required to keep the client's <c>BattleFinishResponsProcessing</c>
/// happy.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public sealed class ColosseumBattleFinishResponseDto
{
    [JsonPropertyName("battle_result")] [Key("battle_result")]
    public int BattleResult { get; set; }

    [JsonPropertyName("get_class_experience")] [Key("get_class_experience")]
    public int GetClassExperience { get; set; }

    [JsonPropertyName("class_experience")] [Key("class_experience")]
    public int ClassExperience { get; set; }

    [JsonPropertyName("class_level")] [Key("class_level")]
    public int ClassLevel { get; set; } = 1;
}
