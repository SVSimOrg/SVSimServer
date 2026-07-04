using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

/// <summary>Mirrors <c>seeds/avatar-abilities.json</c>. One row per leader_skin_id.</summary>
public sealed class AvatarAbilitySeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("battle_start_first_player_turn_bp")] public int BattleStartFirstPlayerTurnBp { get; set; }
    [JsonPropertyName("battle_start_second_player_turn_bp")] public int BattleStartSecondPlayerTurnBp { get; set; }
    [JsonPropertyName("battle_start_max_life")] public int BattleStartMaxLife { get; set; }
    [JsonPropertyName("ability_cost")] public string AbilityCost { get; set; } = "";
    [JsonPropertyName("ability")] public string Ability { get; set; } = "";
    [JsonPropertyName("passive_ability")] public string PassiveAbility { get; set; } = "";
    [JsonPropertyName("ability_desc")] public string AbilityDesc { get; set; } = "";
    [JsonPropertyName("passive_ability_desc")] public string PassiveAbilityDesc { get; set; } = "";
}
