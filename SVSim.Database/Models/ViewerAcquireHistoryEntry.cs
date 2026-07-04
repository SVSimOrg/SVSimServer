namespace SVSim.Database.Models;

/// <summary>
/// One row per grant emitted by <c>InventoryTransaction.CommitAsync</c>. Rendered as the
/// <c>histories[]</c> array on <c>POST /item_acquire_history/info</c>. Capped at 300 rows
/// per viewer; oldest pruned on commit.
/// </summary>
public sealed class ViewerAcquireHistoryEntry
{
    public long Id { get; set; }
    public long ViewerId { get; set; }

    /// <summary>UserGoodsType cast to int; matches the wire <c>reward_type</c>.</summary>
    public int RewardType { get; set; }

    /// <summary>Detail id for the goods; 0 for wallet currencies.</summary>
    public long RewardDetailId { get; set; }

    /// <summary>Delta granted in this row — NOT a post-state total.</summary>
    public int RewardCount { get; set; }

    /// <summary>GrantSource cast to int; matches the wire <c>acquire_type</c>.</summary>
    public int AcquireType { get; set; }

    /// <summary>Pre-localized text the client renders verbatim. Capped at 64 chars.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Server UTC at commit time. Stamped once per <c>CommitAsync</c>, identical across all rows in that commit.</summary>
    public DateTime AcquireTime { get; set; }
}
