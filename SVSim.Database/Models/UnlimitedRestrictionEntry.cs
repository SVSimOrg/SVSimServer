using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Per-card unlimited-format ban/limit value from /load/index data.unlimited_restricted_base_card_id_list
/// (dict {card_id: restriction_value}). RestrictionValue semantics TBD — prod observed {0, 1}; the audit
/// flags this as "0 = limit-1? 1 = hard-ban?" pending a client read.
/// </summary>
public class UnlimitedRestrictionEntry : BaseEntity<long>
{
    public long CardId { get => Id; set => Id = value; }

    public int RestrictionValue { get; set; }
}
