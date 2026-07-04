using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per pack covered by drawrates data. PK is the pack id (matches PackConfigEntry.Id
/// for live-capture rows; standalone for archive-only packs). Weak relationship — PackDraw rows
/// exist for all archived packs even when no PackConfigEntry is enabled.
/// </summary>
public class PackDrawConfigEntry : BaseEntity<int>
{
    public double AnimationRatePct { get; set; }
    public bool HasBonusSlot { get; set; }
    public string? SpecialKind { get; set; }
    public string? ShortCode { get; set; }
}
