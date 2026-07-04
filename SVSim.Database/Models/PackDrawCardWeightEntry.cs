using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// Per-card-rate fact: which card prints in which (pack, slot, tier) at what rate.
/// RatePct is nullable for rate-less "Guaranteed Leader Card" rows (sampler uses
/// "uniform over (pool minus owned)" in that case).
/// ClassId is nullable: null means the row applies to any class draw (the default
/// for non-rotation-starter packs), an integer 1..8 means the row only applies
/// when the viewer selects that class. RotationStarterCardPack rows always carry
/// a non-null ClassId; cards shared across multiple classes are duplicated per
/// class.
/// </summary>
public class PackDrawCardWeightEntry : BaseEntity<long>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override long Id { get; set; }

    public int PackId { get; set; }
    public DrawSlot Slot { get; set; }
    public DrawTier Tier { get; set; }
    public int? ClassId { get; set; }
    public long CardId { get; set; }
    public double? RatePct { get; set; }
    public bool IsLeader { get; set; }
    public bool IsAltArt { get; set; }
}
