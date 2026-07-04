using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row of the Steam/PC storefront item list from /payment_pc/item_list data. Singleton per
/// product. Id is the wire's <c>record_id</c> (prod's auto-increment, genuinely stable across
/// captures — same upsert-by-wire-id pattern as MasterPointRankingPeriodEntry, not the synthetic-
/// ordinal Banner pattern).
///
/// All numeric fields land in typed columns; the controller stringifies them on the way out to
/// match prod's PHP-stringified wire convention.
/// </summary>
public class PaymentItemEntry : BaseEntity<int>
{
    /// <summary>Internal product id (different from store_product_id). Used by the client at
    /// PaymentItemListTask.cs:50,58,64,67 as a per-tier discriminator.</summary>
    public int ProductId { get; set; }

    /// <summary>User-visible SKU (e.g. 10011 for "60-crystal set"). Wire dict key.</summary>
    public long StoreProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int ChargeCrystalNum { get; set; }

    public int FreeCrystalNum { get; set; }

    public int PurchaseLimit { get; set; }

    /// <summary>0/1 — special_shop_flag on the wire (stringified).</summary>
    public int SpecialShopFlag { get; set; }

    public string ImageName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int RemainingTime { get; set; }

    /// <summary>0/1 — is_resale_product on the wire (stringified).</summary>
    public int IsResaleProduct { get; set; }

    /// <summary>Nullable — prod sends empty string when unset; we store null and emit "".</summary>
    public DateTime? ResaleStartDate { get; set; }
}
