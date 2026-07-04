using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// Per (pack, slot, tier) rate. Natural key (PackId, Slot, Tier) is enforced via unique index.
/// Id is auto-generated — override BaseEntity's [DatabaseGenerated(None)] default.
/// </summary>
public class PackDrawSlotRateEntry : BaseEntity<long>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override long Id { get; set; }

    public int PackId { get; set; }
    public DrawSlot Slot { get; set; }
    public DrawTier Tier { get; set; }
    public double RatePct { get; set; }
}
