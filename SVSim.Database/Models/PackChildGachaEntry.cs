using Microsoft.EntityFrameworkCore;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One sub-option inside a pack (single-open / 10-open / ticket / daily-free).
/// Wire shape: one entry of <c>child_gacha_info</c> in /pack/info. Owned by PackConfigEntry.
/// </summary>
[Owned]
public class PackChildGachaEntry
{
    public int GachaId { get; set; }
    public CardPackType TypeDetail { get; set; }
    public int Cost { get; set; }
    public int CardCount { get; set; }
    public long? ItemId { get; set; }
    public bool IsDailySingle { get; set; }
    public int OverrideIncreaseGachaPoint { get; set; }
    public int PurchaseLimitCount { get; set; }
    public int DailyFreeGachaCount { get; set; }
    public int? FreeGachaCampaignId { get; set; }
    public string? CampaignName { get; set; }
}
