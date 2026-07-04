using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One purchasable prebuilt-deck product. PK = wire product_id. FK SeriesId.
/// Pricing columns are nullable; either Crystal or Rupy pair (or both, both zero for free) must
/// be populated for an enabled product. The Intro/Regular pair captures the two-tier pricing
/// pattern: Intro applies to the first purchase, Regular to subsequent. For PurchaseNumMax=1
/// products, Regular stays null and only Intro is ever served.
/// </summary>
public class BuildDeckProductEntry : BaseEntity<int>
{
    public int SeriesId { get; set; }
    public int LeaderId { get; set; }
    public string DeckCode { get; set; } = string.Empty;
    public string ProductNameKey { get; set; } = string.Empty;   // BDPN_*
    public long FeaturedCardId { get; set; }
    public int PurchaseNumMax { get; set; }

    public int? IntroPriceCrystal { get; set; }
    public int? RegularPriceCrystal { get; set; }
    public int? IntroPriceRupy { get; set; }
    public int? RegularPriceRupy { get; set; }

    public bool IsEnabled { get; set; }

    public List<BuildDeckProductCardEntry> Cards { get; set; } = new();
    public List<BuildDeckProductRewardEntry> Rewards { get; set; } = new();

    public BuildDeckSeriesEntry? Series { get; set; }
}
