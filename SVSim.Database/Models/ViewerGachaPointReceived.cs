using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Marker row recording that a viewer has already redeemed <c>CardId</c> from <c>PackId</c>'s
/// gacha-point exchange. Drives the per-entry <c>is_received</c> flag in
/// <c>/pack/get_gacha_point_rewards</c>. Owned collection on <see cref="Viewer"/>.
/// Unique index on (ViewerId, PackId, CardId) per project_owned_collection_unique_index.
/// </summary>
[Owned]
public class ViewerGachaPointReceived
{
    public int PackId { get; set; }
    public long CardId { get; set; }
    public DateTime ReceivedAt { get; set; }
}
