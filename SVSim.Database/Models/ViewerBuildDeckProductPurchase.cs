using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer, per-product purchase counter. Owned collection on Viewer.
/// Unique (ViewerId, ProductId) enforced in SVSimDbContext per project_owned_collection_unique_index.
/// </summary>
[Owned]
public class ViewerBuildDeckProductPurchase
{
    public int ProductId { get; set; }
    public int PurchaseCount { get; set; }
}
