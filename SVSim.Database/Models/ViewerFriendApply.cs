namespace SVSim.Database.Models;

/// <summary>
/// One pending friend application. <see cref="Id"/> is the wire <c>apply_id</c>
/// (auto-generated). Unique on <c>(FromViewerId, ToViewerId)</c> — a viewer can only
/// have one outstanding apply to any given target.
/// </summary>
public class ViewerFriendApply
{
    public int Id { get; set; }
    public long FromViewerId { get; set; }
    public long ToViewerId { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Beginner-friend campaign tag. Defaults to 0 (no campaign). Surfaces as optional <c>mission_type</c> on the wire.</summary>
    public int MissionType { get; set; }
}
