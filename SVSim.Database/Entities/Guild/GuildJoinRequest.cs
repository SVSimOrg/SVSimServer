namespace SVSim.Database.Entities.Guild;

public class GuildJoinRequest
{
    public int GuildId { get; set; }
    public long ViewerId { get; set; }
    public GuildJoinRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public Guild Guild { get; set; } = null!;
}
