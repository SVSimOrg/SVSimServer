using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;

namespace SVSim.Database.Services.Inventory;

/// <summary>
/// Scoped builder returned by <see cref="IInventoryService.BeginAsync"/>. Queue spend +
/// grant operations; commit to save and assemble the <see cref="InventoryCommitResult"/>.
/// <para>
/// Dispose without committing rolls back the underlying DB transaction and detaches any
/// in-memory mutations. <b>Always</b> wrap in <c>await using</c>.
/// </para>
/// </summary>
public interface IInventoryTransaction : IAsyncDisposable
{
    Viewer Viewer { get; }
    bool IsFreeplay { get; }

    /// <summary>
    /// Debits one of the four scalar wallets. Freeplay-aware for Crystal/Rupee/RedEther
    /// (returns Success with the configured freeplay amount, balance unchanged); SpotPoint
    /// always real. Returns <see cref="SpendOutcome.Insufficient"/> with current balance on
    /// failure; viewer state is not mutated on failure.
    /// </summary>
    Task<SpendResult> TrySpendAsync(SpendCurrency currency, long cost, CancellationToken ct = default);

    /// <summary>
    /// Type-dispatched debit. Currencies (RedEther/Crystal/Rupy/SpotCardPoint) route to
    /// <see cref="TrySpendAsync"/>; Item decrements <c>OwnedItemEntry.Count</c>. Returns
    /// <see cref="SpendResult"/> whose <c>PostStateTotal</c> is the new wallet balance for
    /// currencies and the remaining item count for Item.
    /// </summary>
    Task<SpendResult> TryDebitAsync(UserGoodsType type, long detailId, int num, CancellationToken ct = default);

    Task<IReadOnlyList<GrantedReward>> GrantAsync(UserGoodsType type, long detailId, int num, CancellationToken ct = default);
    Task<int> BackfillCardCosmeticsAsync(CancellationToken ct = default);

    /// <summary>
    /// Freeplay-aware balance read against the live viewer; reflects any spends queued in
    /// this transaction. Inside a transaction, use this; outside, use
    /// <see cref="IInventoryService.EffectiveBalance"/>.
    /// </summary>
    long EffectiveBalance(SpendCurrency currency);
    bool OwnsCard(long cardId);
    bool OwnsCosmetic(CosmeticType type, int id);

    Task<InventoryCommitResult> CommitAsync(CancellationToken ct = default);
}
