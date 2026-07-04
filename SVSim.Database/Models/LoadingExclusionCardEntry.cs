using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Cards excluded from loading-screen art rotation, from /load/index data.loading_exclusion_card_list.
/// References ShadowverseCardEntry.Id but no FK.
/// </summary>
public class LoadingExclusionCardEntry : BaseEntity<long>
{
    public long CardId { get => Id; set => Id = value; }
}
