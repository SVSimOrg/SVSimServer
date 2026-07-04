using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One purchasable sleeve product. PK = wire product_id (e.g. 301901). FK SeriesId.
/// <para>
/// Both <see cref="PriceCrystal"/> and <see cref="PriceRupy"/> are nullable. At least one must be
/// populated for an enabled product (both zero = free, both null = invalid). Sleeves don't have
/// the two-tier intro/regular pricing that BuildDeck products use — one price per currency.
/// </para>
/// <para>
/// <see cref="Rewards"/> drives both the catalog display (in /sleeve/info) and the actual grant
/// list (in /sleeve/buy). The capture shows each sleeve product grants a sleeve (type=6) and an
/// emblem (type=7) — both faithful reward_detail_ids that exist in the cosmetic catalogs.
/// </para>
/// </summary>
public class SleeveShopProductEntry : BaseEntity<int>
{
    public int SeriesId { get; set; }
    /// <summary>Wire `name` field — SystemText key like "sleeve_138". Localised client-side.</summary>
    public string NameKey { get; set; } = string.Empty;

    public int? PriceCrystal { get; set; }
    public int? PriceRupy { get; set; }

    public bool IsEnabled { get; set; }

    public List<SleeveShopProductRewardEntry> Rewards { get; set; } = new();

    public SleeveShopSeriesEntry? Series { get; set; }
}
