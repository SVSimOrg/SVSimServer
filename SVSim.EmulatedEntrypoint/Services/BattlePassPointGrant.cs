using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Result of <see cref="IBattlePassService.AddPointsAsync"/>. Future point-source endpoints
/// (mission/retire, battle finish handlers) translate this into the embedded
/// <c>battle_pass_gauge_info</c> block on their response.
/// </summary>
public sealed record BattlePassPointGrant(
    int BeforePoint,
    int BeforeLevel,
    int AfterPoint,
    int AfterLevel,
    int PointAdd,
    BattlePassPointSource Source,
    IReadOnlyList<GrantedReward> NewlyClaimed);
