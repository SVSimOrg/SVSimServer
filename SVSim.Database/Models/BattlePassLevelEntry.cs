using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One battle pass level (1..100). Mirrors a single entry in /load/index.battle_pass_level_info.
/// Curve is global, immutable per deploy; cached by IBattlePassService.
/// </summary>
public class BattlePassLevelEntry : BaseEntity<int>
{
    public int Level { get => Id; set => Id = value; }
    public int RequiredPoint { get; set; }
}
