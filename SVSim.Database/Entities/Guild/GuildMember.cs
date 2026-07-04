namespace SVSim.Database.Entities.Guild;

public class GuildMember
{
    public int GuildId { get; set; }
    public long ViewerId { get; set; }
    public GuildRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public Guild Guild { get; set; } = null!;
}
