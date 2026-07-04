using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Result of <see cref="IBattlePassService.BuyPremiumAsync"/>. <c>AchievedRewards</c> = the
/// delta that was just granted (goes into <c>achieved_info.battle_pass_reward_list</c>);
/// <c>PostStateTotals</c> = post-state totals for affected goods (goes into <c>reward_list</c>),
/// including the crystal deduction per memory project_wire_reward_list_post_state.
/// </summary>
public sealed record BattlePassBuyOutcome(
    int ResultCode,
    IReadOnlyList<GrantedReward> AchievedRewards,
    IReadOnlyList<GrantedReward> PostStateTotals);
