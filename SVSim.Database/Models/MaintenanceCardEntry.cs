using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Cards disabled mid-season for emergency balance, from /load/index data.maintenance_card_list.
/// Empty in current prod capture; recapture target if a card ever gets emergency-disabled before EOS.
/// </summary>
public class MaintenanceCardEntry : BaseEntity<long>
{
    public long CardId { get => Id; set => Id = value; }
}
