using SVSim.Database.Enums;
using SVSim.Database.Models;

namespace SVSim.Database.Services;

/// <summary>
/// Wire-shape entry returned by <see cref="Inventory.IInventoryTransaction.GrantAsync"/> and
/// collected in <see cref="Inventory.InventoryCommitResult.RewardList"/> /
/// <see cref="Inventory.InventoryCommitResult.Deltas"/>. Field names match the
/// <c>reward_list</c> entries used by <c>/pack/open</c>, <c>/basic_puzzle/finish</c>, and
/// <c>/story/*/finish</c>. reward_num is a POST-STATE TOTAL for currencies and a count for
/// collection grants — see <see cref="Models.RewardListEntry"/>.
/// </summary>
public sealed record GrantedReward(UserGoodsType RewardType, long RewardId, int RewardNum);

/// <summary>
/// Cosmetic projection bundle for /load/index. The four id-lists are "what the viewer owns"
/// (all of them in freeplay). Leader skins are always the full catalog with a per-skin owned flag;
/// <see cref="OwnedLeaderSkinIds"/> is every skin id in freeplay.
/// </summary>
public sealed record EffectiveCosmetics(
    IReadOnlyList<int> SleeveIds,
    IReadOnlyList<int> EmblemIds,
    IReadOnlyList<int> DegreeIds,
    IReadOnlyList<int> MyPageBackgroundIds,
    IReadOnlyList<LeaderSkinEntry> AllLeaderSkins,
    IReadOnlySet<int> OwnedLeaderSkinIds);
