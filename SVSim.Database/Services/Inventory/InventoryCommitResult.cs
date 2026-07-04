using SVSim.Database.Services;

namespace SVSim.Database.Services.Inventory;

/// <summary>
/// Result of <see cref="IInventoryTransaction.CommitAsync"/>.
/// <para>
/// <see cref="RewardList"/> — wire-shape entries with currency-collision resolved (one entry per
/// (type, id); for currencies that were both spent and granted, the last post-state in op order
/// wins). Use this for response <c>reward_list</c> fields.
/// </para>
/// <para>
/// <see cref="Deltas"/> — verbatim ordered (type, id, num) sequence the caller queued. No
/// collapse, no cosmetic-cascade entries. Use this for BP <c>achieved_info</c> and Story
/// <c>story_reward_list</c> popups.
/// </para>
/// </summary>
public sealed record InventoryCommitResult(
    IReadOnlyList<GrantedReward> RewardList,
    IReadOnlyList<GrantedReward> Deltas);
