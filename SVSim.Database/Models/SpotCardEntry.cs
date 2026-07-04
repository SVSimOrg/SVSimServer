using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per rentable "spot card" from /load/index data.spot_cards (dict {card_id: cost}).
/// References ShadowverseCardEntry.Id but no FK — bootstrap warns on orphans.
/// </summary>
public class SpotCardEntry : BaseEntity<long>
{
    public long CardId { get => Id; set => Id = value; }

    public int Cost { get; set; }
}
