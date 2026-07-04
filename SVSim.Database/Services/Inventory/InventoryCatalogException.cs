namespace SVSim.Database.Services.Inventory;

/// <summary>
/// Thrown when an inventory operation references a catalog id that doesn't exist
/// (unknown card / item / cosmetic). Programmer error — bubbles to the global error handler.
/// </summary>
public sealed class InventoryCatalogException : Exception
{
    public InventoryCatalogException(string message) : base(message) { }
}
