namespace SVSim.Database.Services.Inventory;

/// <summary>
/// Shared knobs for the viewer-acquire-history audit log. The write-side prune cap
/// (in <c>InventoryTransaction</c>) and the read-side page size (in
/// <c>ItemAcquireHistoryController</c>) both reference these constants so they cannot drift.
/// </summary>
public static class InventoryHistoryConfig
{
    /// <summary>
    /// Maximum rows kept per viewer. Older rows are pruned by
    /// <c>InventoryTransaction.CommitAsync</c>; the read endpoint pages exactly this many.
    /// </summary>
    public const int RetentionRowsPerViewer = 300;
}
