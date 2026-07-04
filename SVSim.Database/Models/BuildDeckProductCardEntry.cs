using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// One card in a prebuilt-deck product's 40-card list. Owned by BuildDeckProductEntry.
/// Shape mirrors `build_deck_package_master.csv` rows: (ProductId, CardId, Number, IsSpot).
/// IsSpot=true marks the special prize/featured cards rendered in the separate _spotCardRoot
/// panel by BuildDeckProductDetail.cs.
/// </summary>
[Owned]
public class BuildDeckProductCardEntry
{
    public long CardId { get; set; }
    public int Number { get; set; }
    public bool IsSpot { get; set; }
}
