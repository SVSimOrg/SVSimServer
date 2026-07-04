namespace SVSim.Database.Entities.Guild;

public class GuildChatMessage
{
    public long Id { get; set; }                    // global PK
    public int GuildId { get; set; }
    public int MessageId { get; set; }              // per-guild monotonic, unique with GuildId
    public long AuthorViewerId { get; set; }
    public GuildChatMessageType MessageType { get; set; }
    public string Body { get; set; } = "";
    public string? DeckPayload { get; set; }        // jsonb
    public string? ReplayPayload { get; set; }      // jsonb
    public string? RoomPayload { get; set; }        // jsonb
    public DateTime CreatedAt { get; set; }
    public Guild Guild { get; set; } = null!;
}
