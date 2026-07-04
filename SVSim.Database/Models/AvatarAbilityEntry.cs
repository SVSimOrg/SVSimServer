using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One Avatar (Hero) mode definition. Keyed by leader_skin_id. The Ability/PassiveAbility strings
/// are the dense "(skill:...)(timing:...)" effect DSL that cannot be reconstructed from card master —
/// preserve verbatim from /load/index data.avatar_info.abilities[leaderSkinId].
/// </summary>
public class AvatarAbilityEntry : BaseEntity<int>
{
    public int LeaderSkinId { get => Id; set => Id = value; }

    public int BattleStartFirstPlayerTurnBp { get; set; }

    public int BattleStartSecondPlayerTurnBp { get; set; }

    public int BattleStartMaxLife { get; set; }

    public string AbilityCost { get; set; } = string.Empty;

    public string Ability { get; set; } = string.Empty;

    public string PassiveAbility { get; set; } = string.Empty;

    public string AbilityDesc { get; set; } = string.Empty;

    public string PassiveAbilityDesc { get; set; } = string.Empty;
}
