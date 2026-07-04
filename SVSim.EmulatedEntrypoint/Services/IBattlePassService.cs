using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

namespace SVSim.EmulatedEntrypoint.Services;

public interface IBattlePassService
{
    /// <summary>
    /// Plumbing for future point-source endpoints (mission/retire, battle finish handlers).
    /// Bumps gauge by min(amount, weekly headroom), auto-grants rewards on every level crossed
    /// (premium track only when IsPremium), writes claim rows + currency/collection mutations
    /// via RewardGrantService. Returns delta info for caller to embed in
    /// battle_pass_gauge_info on its response. No live caller in v1; tested directly.
    /// </summary>
    Task<BattlePassPointGrant> AddPointsAsync(
        long viewerId, BattlePassPointSource source, int amount, CancellationToken ct);

    /// <summary>Global level curve as wire-string dictionary; null if no levels seeded.</summary>
    Task<IReadOnlyDictionary<string, BattlePassLevel>?> GetLevelCurveAsync(CancellationToken ct);

    /// <summary>
    /// /battle_pass/info payload. Returns null when no active season window covers <c>now</c>
    /// (controller emits empty body in that case).
    /// </summary>
    Task<BattlePassInfoResponse?> GetInfoAsync(long viewerId, CancellationToken ct);

    /// <summary>
    /// /battle_pass/item_list payload. Returns one product per active season; empty products
    /// array if the viewer already owns premium for the active season. Null when no active season.
    /// </summary>
    Task<BattlePassItemListResponse?> GetItemListAsync(long viewerId, CancellationToken ct);

    /// <summary>
    /// Purchase premium for the active season. Validates season_id matches active season,
    /// product id derives from season, viewer has crystals, viewer isn't already premium.
    /// On success: deducts crystals, flips IsPremium, retroactively grants premium rewards for
    /// every level &lt;= current_level not yet claimed. All-or-nothing transaction.
    /// </summary>
    Task<BattlePassBuyOutcome> BuyPremiumAsync(long viewerId, int seasonId, int productId, CancellationToken ct);
}
