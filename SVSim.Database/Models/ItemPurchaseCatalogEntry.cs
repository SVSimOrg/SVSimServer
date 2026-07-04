using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row of the /item_purchase/info catalog — an exchange the user can perform N times per
/// period (monthly or lifetime) by spending <c>RequireItem*</c> to acquire <c>PurchaseItem*</c>.
/// PK = wire <c>purchase_id</c>.
/// <para>
/// Both sides reference <see cref="Enums.UserGoodsType"/>. Captures show the common shape is
/// currency-for-item (RedEther 5000 → Seer's Globe ×1) or item-for-item (Orb Shard ×5 →
/// Seer's Globe ×1). Per-viewer remaining quota lives in
/// <see cref="ViewerEventCounter"/> keyed by <c>"item_purchase:{Id}"</c>.
/// </para>
/// </summary>
public class ItemPurchaseCatalogEntry : BaseEntity<int>
{
    public int RequireItemType { get; set; }
    public long RequireItemId { get; set; }
    public int RequireItemNum { get; set; }

    public int PurchaseItemType { get; set; }
    public long PurchaseItemId { get; set; }
    public int PurchaseItemNum { get; set; }

    /// <summary>
    /// SystemText-ready display name. May be empty — the client falls back to a templated name
    /// built from <c>UserGoods.getUserGoodsName + count</c> via SystemText key "Shop_0132".
    /// </summary>
    public string PurchaseName { get; set; } = string.Empty;

    /// <summary>True → quota resets at the start of each JST month. False → lifetime quota.</summary>
    public bool IsMonthlyReset { get; set; }

    /// <summary>Per-period purchase cap. Wire <c>rest</c> = max(0, PurchaseLimit - counter).</summary>
    public int PurchaseLimit { get; set; }

    public bool IsEnabled { get; set; }
}
