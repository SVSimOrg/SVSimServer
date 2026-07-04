using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.EmulatedEntrypoint.Services;

public interface IGachaPointService
{
    /// <summary>
    /// Build the gacha-point exchange catalog for one pack, with per-viewer is_received
    /// resolved. Returns an empty list if the pack has no gacha-point config or no eligible
    /// cards in its pool — callers should treat the empty result as a valid response, not
    /// an error. Order: standard legendaries first (class_id ASC, card_id ASC), then leader
    /// cards (class_id ASC, card_id ASC).
    /// </summary>
    Task<IReadOnlyList<GachaPointRewardDto>> GetRewardsAsync(int packId, long viewerId);

    /// <summary>
    /// Increment the viewer's balance for <paramref name="pack"/> by
    /// <c>child.OverrideIncreaseGachaPoint > 0 ? child.OverrideIncreaseGachaPoint : pack.GachaPointConfig.IncreaseGachaPoint</c>
    /// times <paramref name="packNumber"/>. No-op when the pack lacks a GachaPointConfig.
    /// Caller is responsible for SaveChangesAsync.
    /// </summary>
    void Accrue(Viewer viewer, PackConfigEntry pack, PackChildGachaEntry child, int packNumber);

    /// <summary>
    /// Validate + execute an exchange using the provided inventory transaction (which must
    /// have <c>GachaPointBalances</c> and <c>GachaPointReceived</c> loaded on <c>tx.Viewer</c>
    /// via <see cref="IInventoryService.BeginAsync"/> extra includes). Grants the card via
    /// the tx. Returns the grant outcome on success (reward_list entries already converted to
    /// <see cref="RewardListEntry"/>), or a failure result describing why. Caller commits
    /// the tx on success.
    /// </summary>
    Task<ExchangeOutcome> TryExchangeAsync(IInventoryTransaction tx, int packId, long cardId);
}

public sealed record ExchangeOutcome(bool Success, string? Error, IReadOnlyList<RewardListEntry> RewardList)
{
    public static ExchangeOutcome Fail(string error) => new(false, error, Array.Empty<RewardListEntry>());
    public static ExchangeOutcome Ok(IReadOnlyList<RewardListEntry> rewards) => new(true, null, rewards);
}
