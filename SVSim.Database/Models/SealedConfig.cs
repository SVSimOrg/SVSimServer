using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Singleton row (Id=1) for the current Sealed Arena season from /mypage/index data.sealed_info.
/// PackInfo jsonb is the int[] of pack set IDs used in the pool.
/// </summary>
public class SealedConfig : BaseEntity<int>
{
    public int Enable { get; set; }

    public int CrystalCost { get; set; }

    public int RupyCost { get; set; }

    public int TicketCost { get; set; }

    public int DeckUsingNumMin { get; set; }

    public int ScheduleId { get; set; }

    public bool IsJoin { get; set; }

    public bool IsDeckCodeMaintenance { get; set; }

    [Column(TypeName = "jsonb")]
    public string PackInfo { get; set; } = "[]";

    [Column(TypeName = "jsonb")]
    public string SalesPeriodInfo { get; set; } = "{}";
}
