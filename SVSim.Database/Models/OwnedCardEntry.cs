using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Represents viewer ownership of a <see cref="ShadowverseCardEntry"/>.
/// </summary>
[Owned]
public class OwnedCardEntry
{
    /// <summary>
    /// Game rule: a viewer may own at most this many copies of a single card. Mirrors the
    /// client constant <c>CardMake.CAN_CREATE_MAX = 3</c>. Used by <c>/card/create</c> to
    /// reject batches that would exceed the cap.
    /// </summary>
    public const int MaxCopies = 3;

    public ShadowverseCardEntry Card { get; set; } = new ShadowverseCardEntry();
    public int Count { get; set; }
    public bool IsProtected { get; set; }
}
