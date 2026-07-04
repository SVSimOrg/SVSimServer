using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public interface IGuildChatMessageRepository
{
    /// <summary>Atomically allocate the next per-guild message_id and insert.</summary>
    Task<GuildChatMessage> AppendAsync(GuildChatMessage msg, CancellationToken ct = default);

    /// <summary>Window query. direction: 1=OLD (asc-walk older), 2=NEW (newer), 3=BOTH (around).
    /// `start` may be 0 meaning "latest". Returns ordered oldest-to-newest.</summary>
    Task<IReadOnlyList<GuildChatMessage>> GetWindowAsync(int guildId, int start, int direction, int limit, CancellationToken ct = default);

    Task<int> GetMaxMessageIdAsync(int guildId, CancellationToken ct = default);

    /// <summary>
    /// Returns the highest message_id for the guild, or null if no messages exist.
    /// Adapts <see cref="GetMaxMessageIdAsync"/> (which returns 0 when empty) for callers
    /// that need null-vs-present semantics (e.g. GuildNotification.guild_room_message_id).
    /// </summary>
    Task<long?> GetMaxMessageIdSafelyAsync(int guildId, CancellationToken ct = default);

    Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default);

    /// <summary>Fetch a single message by (guildId, messageId). Returns null if not found.</summary>
    Task<GuildChatMessage?> GetByMessageIdAsync(int guildId, int messageId, CancellationToken ct = default);

    /// <summary>
    /// Fetch all DECK-type messages for a guild (non-null DeckPayload only — clears are excluded).
    /// </summary>
    Task<IReadOnlyList<GuildChatMessage>> GetDeckMessagesAsync(int guildId, CancellationToken ct = default);

    /// <summary>
    /// For /guild_chat/delete_deck: clears the deck payload + body without removing the message row,
    /// so the slot remains in the timeline ("[deleted]"). Returns false if not found / not deletable by caller.
    /// <paramref name="leaderOverride"/> bypasses the author check — pass true when caller is Leader or SubLeader.
    /// </summary>
    Task<bool> ClearDeckAsync(int guildId, int messageId, long callerViewerId, bool leaderOverride, CancellationToken ct = default);
}
