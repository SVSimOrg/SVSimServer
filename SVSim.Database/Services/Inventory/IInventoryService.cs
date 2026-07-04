using SVSim.Database.Models;
using SVSim.Database.Services;

namespace SVSim.Database.Services.Inventory;

public interface IInventoryService
{
    /// <summary>
    /// Loads the viewer with the canonical inventory graph (Cards.Card, Sleeves, Emblems,
    /// LeaderSkins, Degrees, MyPageBackgrounds, Items.Item under AsSplitQuery), opens a DB
    /// transaction, and returns a builder for queueing operations. Throws
    /// <see cref="InventoryViewerNotFoundException"/> if the viewer does not exist.
    /// </summary>
    Task<IInventoryTransaction> BeginAsync(
        long viewerId,
        CancellationToken ct = default,
        Action<InventoryLoadConfig>? configure = null);

    Task<IReadOnlyList<OwnedCardEntry>> EffectiveOwnedCardsAsync(Viewer viewer, CancellationToken ct = default);
    Task<EffectiveCosmetics> EffectiveCosmeticsAsync(Viewer viewer, CancellationToken ct = default);
    long EffectiveBalance(Viewer viewer, SpendCurrency currency);
}

public sealed class InventoryViewerNotFoundException : Exception
{
    public InventoryViewerNotFoundException(long viewerId)
        : base($"Viewer {viewerId} not found") { }
}
