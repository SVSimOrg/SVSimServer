namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

/// <summary>
/// What the battle controller needs from the service to build the standard battle-finish
/// envelope (class XP delta + post-state, spot points before/add/after, battle_result echo).
/// Achieved-info is built by the controller from IMissionAssembler.
/// <see cref="LeveledUp"/> and <see cref="ClassId"/> are controller-side signals for the
/// class_level_up mission emit and are not serialized to the wire.
/// </summary>
public class BattleFinishResultDto
{
    public int BattleResult { get; set; }
    public int GetClassExperience { get; set; }
    public int ClassExperience { get; set; }
    public int ClassLevel { get; set; }
    public int BeforeSpotPoint { get; set; }
    public int AddSpotPoint { get; set; }
    public int AfterSpotPoint { get; set; }
    public bool LeveledUp { get; set; }
    public int ClassId { get; set; }
}
