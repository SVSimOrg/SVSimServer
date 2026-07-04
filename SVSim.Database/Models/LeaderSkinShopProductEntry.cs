using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One purchasable leader-skin product. PK = wire product_id (small ints in captures — e.g. 31,
/// 165, 166). FK <see cref="SeriesId"/>. <see cref="LeaderSkinId"/> points at the
/// <see cref="LeaderSkinEntry"/> the buyer ends up owning.
/// </summary>
public class LeaderSkinShopProductEntry : BaseEntity<int>
{
    public int SeriesId { get; set; }
    public int LeaderSkinId { get; set; }

    /// <summary>SystemText keys — resolved client-side via Data.Master.GetLeaderSkinProductText.</summary>
    public string ProductNameKey { get; set; } = string.Empty;
    public string IntroductionKey { get; set; } = string.Empty;
    public string CvNameKey { get; set; } = string.Empty;

    /// <summary>
    /// Per-product price for solo buy. Captures consistently show crystal/rupy parity for
    /// regular skins (500c / 500r single, 400 unit-price when bought as set). Nullable so
    /// promotions can offer one currency without the other.
    /// </summary>
    public int? SinglePriceCrystal { get; set; }
    public int? SinglePriceRupy { get; set; }
    public int? SinglePriceTicket { get; set; }
    public int? TicketNumber { get; set; }
    public long? TicketItemId { get; set; }

    public bool IsEnabled { get; set; }

    public List<LeaderSkinShopProductRewardEntry> Rewards { get; set; } = new();

    public LeaderSkinShopSeriesEntry? Series { get; set; }
}
