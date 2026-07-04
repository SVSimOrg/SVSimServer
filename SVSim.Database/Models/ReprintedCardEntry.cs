using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Cards currently in the reprinted list from /load/index data.reprinted_base_card_ids.
/// References ShadowverseCardEntry.Id but no FK.
/// </summary>
public class ReprintedCardEntry : BaseEntity<long>
{
    public long CardId { get => Id; set => Id = value; }
}
