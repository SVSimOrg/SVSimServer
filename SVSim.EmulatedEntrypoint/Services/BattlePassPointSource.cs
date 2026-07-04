namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Which categorized field on <c>battle_pass_gauge_info</c> a point grant feeds. Mirrors the
/// breakdown shown by <c>BattlePassResultPanel</c> after a win.
/// </summary>
public enum BattlePassPointSource
{
    BattleResult,
    DailyMission,
    BattlePassMission,
}
