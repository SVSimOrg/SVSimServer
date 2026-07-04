using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class AvatarAbility
{
    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")]
    public int LeaderSkinId { get; set; }
    [JsonPropertyName("battle_start_firstplayerturn_bp")]
    [Key("battle_start_firstplayerturn_bp")]
    public int BattleStartFirstPlayerBp { get; set; }
    [JsonPropertyName("battle_start_secondplayerturn_bp")]
    [Key("battle_start_secondplayerturn_bp")]
    public int BattleStartSecondPlayerBp { get; set; }
    [JsonPropertyName("battle_start_max_life")]
    [Key("battle_start_max_life")]
    public int BattleStartMaxLife { get; set; }
    [JsonPropertyName("ability_cost")]
    [Key("ability_cost")]
    public string AbilityCost { get; set; }
    [JsonPropertyName("ability")]
    [Key("ability")]
    public string Ability { get; set; }
    [JsonPropertyName("passive_ability")]
    [Key("passive_ability")]
    public string PassiveAbility { get; set; }
    [JsonPropertyName("ability_desc")]
    [Key("ability_desc")]
    public string AbilityDesc { get; set; }
    [JsonPropertyName("passive_ability_desc")]
    [Key("passive_ability_desc")]
    public string PassiveAbilityDesc { get; set; }
}