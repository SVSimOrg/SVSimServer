namespace SVSim.Database.Entities.Guild;

public class GuildInvite
{
    /// <summary>
    /// Auto-increment surrogate PK. Used as the wire <c>invite_id</c> returned to the client
    /// and echoed back in cancel_invite / reject_invite requests.
    /// </summary>
    public long Id { get; set; }

    public int GuildId { get; set; }
    public long InviteeViewerId { get; set; }
    public long InviterViewerId { get; set; }
    public GuildInviteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public Guild Guild { get; set; } = null!;
}
