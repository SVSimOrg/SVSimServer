using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One sleeve-shop series (a themed collection — e.g. series 3019 "BattlePass sleeves",
/// series 3004 "Granblue Fantasy collab"). PK = wire series_id. IsEnabled gates whether
/// /sleeve/info renders this series.
/// </summary>
public class SleeveShopSeriesEntry : BaseEntity<int>
{
    public bool IsNew { get; set; }
    public bool IsEnabled { get; set; }

    public List<SleeveShopProductEntry> Products { get; set; } = new();
}
