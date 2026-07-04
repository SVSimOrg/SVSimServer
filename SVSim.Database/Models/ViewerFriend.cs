namespace SVSim.Database.Models;

/// <summary>
/// One row per direction of a friendship. Approving an apply creates two rows
/// (A → B and B → A). <see cref="FriendViewerId"/> from a played-together row
/// can be self-joined against this table to detect an existing friendship.
/// </summary>
public class ViewerFriend
{
    public long OwnerViewerId { get; set; }
    public long FriendViewerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
