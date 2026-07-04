using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Singleton row (Id=1) capturing the current Take Two arena season config from
/// /load/index data.arena_info[0]. FormatInfo jsonb holds the nested
/// {two_pick_type, card_pool_name, announce_id, last_card_pack_set_id, start_time, end_time}.
/// </summary>
public class ArenaSeasonConfig : BaseEntity<int>
{
    public int Mode { get; set; }

    public int Enable { get; set; }

    public ulong Cost { get; set; }

    public ulong RupyCost { get; set; }

    public int TicketCost { get; set; }

    public bool IsJoin { get; set; }

    [Column(TypeName = "jsonb")]
    public string FormatInfo { get; set; } = "{}";
}
