using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;

[MessagePackObject]
public class BattleFinishRequest : BaseRequest
{
    [JsonPropertyName("class_id")] [Key("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("total_turn")] [Key("total_turn")] public int TotalTurn { get; set; }
    [JsonPropertyName("evolve_count")] [Key("evolve_count")] public int EvolveCount { get; set; }
    [JsonPropertyName("enemy_evolve_count")] [Key("enemy_evolve_count")] public int EnemyEvolveCount { get; set; }
    [JsonPropertyName("battle_result")] [Key("battle_result")] public int BattleResult { get; set; }
    [JsonPropertyName("is_retire")] [Key("is_retire")] public int IsRetire { get; set; }
    [JsonPropertyName("SDTRB")] [Key("SDTRB")] public int Sdtrb { get; set; }
}
