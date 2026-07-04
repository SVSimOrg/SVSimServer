using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One leader-skin-shop series (a themed collection — e.g. "7th Anniversary Skins").
/// PK = wire series_id. <see cref="SetSalesStatus"/> controls whether the per-series
/// "buy whole set" UI is offered: 0=none (single-skin purchases only), non-zero=set sale active.
/// When set-active, the set-price + set-completion-reward fields are populated.
/// </summary>
public class LeaderSkinShopSeriesEntry : BaseEntity<int>
{
    public bool IsNew { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>SkinSeriesPurchaseInfo.eSetSalesStatus — 0=None.</summary>
    public int SetSalesStatus { get; set; }

    public int? SetPriceCrystal { get; set; }
    public int? SetPriceRupy { get; set; }
    public int? SetPriceTicket { get; set; }
    public long? SetPriceTicketId { get; set; }

    /// <summary>
    /// SkinSeriesPurchaseInfo.RewardStatus — 0=none. The per-VIEWER claim state is computed
    /// at request time from <see cref="ViewerLeaderSkinSetClaim"/>; this column is the catalog
    /// default surfaced when no viewer is in context (or when set_sales_status==0).
    /// </summary>
    public int SetCompletionRewardStatus { get; set; }

    public List<LeaderSkinShopProductEntry> Products { get; set; } = new();
    public List<LeaderSkinShopSeriesRewardEntry> SetCompletionRewards { get; set; } = new();
}
