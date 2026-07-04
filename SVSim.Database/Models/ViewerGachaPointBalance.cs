using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer, per-pack gacha-point balance. Owned collection on <see cref="Viewer"/>.
/// <c>PackId</c> = parent_gacha_id. <c>Points</c> accumulates one per pack opened (or
/// <c>PackChildGachaEntry.OverrideIncreaseGachaPoint</c> when set on the child) and is
/// decremented by <see cref="PackGachaPointConfig.ExchangeablePoint"/> per exchange.
/// Unique index on (ViewerId, PackId) per project_owned_collection_unique_index.
/// </summary>
[Owned]
public class ViewerGachaPointBalance
{
    public int PackId { get; set; }
    public int Points { get; set; }
}
