using SVSim.Database.Models;

namespace SVSim.Database.Entities.Guild;

public class Guild
{
    public int GuildId { get; set; }                    // server-generated 9-digit
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public long? LeaderViewerId { get; set; }
    public long EmblemId { get; set; }
    public GuildActivity Activity { get; set; }
    public GuildJoinCondition JoinCondition { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? BreakupAt { get; set; }            // soft-delete on breakup
    public List<GuildMember> Members { get; set; } = new();
}
