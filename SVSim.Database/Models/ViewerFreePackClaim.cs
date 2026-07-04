using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// One row per (viewer, free_gacha_campaign_id). Counts claims and remembers when the last one
/// landed so the controller can gate the daily quota. Owned collection on <see cref="Viewer"/>.
/// </summary>
[Owned]
public class ViewerFreePackClaim
{
    public int FreeGachaCampaignId { get; set; }
    public int ClaimCount { get; set; }
    public DateTime LastClaimedAt { get; set; }
}
