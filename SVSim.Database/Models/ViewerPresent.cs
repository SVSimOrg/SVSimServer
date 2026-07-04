namespace SVSim.Database.Models;

/// <summary>
/// One row per gift in a viewer's inbox. Replaces the tutorial-only
/// <c>ViewerClaimedTutorialGift</c> receipts model with a unified status-enum row that
/// serves both /gift/top + /gift/receive_gift (prod) and /tutorial/gift_top +
/// /tutorial/gift_receive (tutorial alias).
/// </summary>
public class ViewerPresent
{
    public long Id { get; set; }

    public long ViewerId { get; set; }
    public Viewer Viewer { get; set; } = null!;

    /// <summary>Wire id ("71409625" in the prod capture). String to match the wire.</summary>
    public string PresentId { get; set; } = string.Empty;

    public PresentStatus Status { get; set; }

    /// <summary>UserGoodsType-compatible int. Wire is stringified — see PresentMapper.</summary>
    public int RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public long RewardCount { get; set; }
    public int ConditionNumber { get; set; }
    public int PresentLimitType { get; set; }
    public long RewardLimitTime { get; set; }
    public int? ItemType { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }

    /// <summary>
    /// Free-form provenance tag for future producers ("tutorial", "challenge_win",
    /// "payment_refund:&lt;txid&gt;", "event:&lt;id&gt;"). Nothing in the receive handler reads
    /// this today — the tutorial-step advance is route-gated, not Source-gated.
    /// </summary>
    public string? Source { get; set; }
}

public enum PresentStatus : byte
{
    Unclaimed = 0,
    Claimed = 1,
    Deleted = 2,
    Expired = 3,
}
