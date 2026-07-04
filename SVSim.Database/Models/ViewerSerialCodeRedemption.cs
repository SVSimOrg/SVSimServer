namespace SVSim.Database.Models;

/// <summary>
/// One row per (viewer, code) redemption. Composite PK on <c>(ViewerId, SerialCodeId)</c>
/// enforces the single-use-per-viewer guarantee at the DB layer; the controller catches
/// the unique-constraint violation as a race-condition backstop.
/// </summary>
public class ViewerSerialCodeRedemption
{
    public long ViewerId { get; set; }
    public int SerialCodeId { get; set; }
    public DateTime RedeemedAt { get; set; }
}
