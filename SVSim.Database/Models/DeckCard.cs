using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// A <see cref="ShadowverseCardEntry"/> that appears in a <see cref="ShadowverseDeckEntry"/> N times.
/// </summary>
[Owned]
public class DeckCard
{
    public ShadowverseCardEntry Card { get; set; } = new ShadowverseCardEntry();

    public int Count { get; set; }
}
