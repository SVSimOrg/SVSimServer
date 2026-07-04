using SVSim.Database.Enums;

namespace SVSim.Database.Services.Guild;

public sealed record ChatWindow(IReadOnlyList<Entities.Guild.GuildChatMessage> Messages, int WaitIntervalSeconds);
public sealed record ChatPostResult(bool Ok, int? AssignedMessageId);

/// <summary>
/// A single entry in the deck log: the raw JSON stored in DeckPayload, plus the envelope fields
/// (messageId, authorViewerId) needed to compute delete_permission_exists in the controller.
/// </summary>
public sealed record DeckLogEntry(int MessageId, long AuthorViewerId, string PayloadJson);

/// <summary>
/// Result for /guild_chat/deck_log and /guild_chat/delete_deck.
/// All active (non-deleted) DECK messages in the guild — caller is responsible for grouping by
/// deck_format and computing delete_permission_exists per entry.
/// </summary>
public sealed record DeckLogResult(IReadOnlyList<DeckLogEntry> Entries);

public interface IGuildChatService
{
    Task<ChatWindow> GetWindowAsync(long viewerId, int startMessageId, int direction, int waitIntervalHint, CancellationToken ct = default);
    Task<ChatPostResult> PostTextOrStampAsync(long viewerId, int type, string message, CancellationToken ct = default);

    /// <summary>
    /// Share a deck snapshot to guild chat. The deck is looked up by (viewerId, format, deckNo)
    /// and its current state is stored as the DECK message's DeckPayload JSON.
    /// <paramref name="deckFormatApi"/> is the API-side wire integer (used verbatim in the stored payload).
    /// </summary>
    Task<ChatPostResult> PostDeckAsync(long viewerId, Format format, int deckFormatApi, int deckNo, CancellationToken ct = default);

    /// <summary>
    /// Delete a shared deck. Caller must be the author OR a leader/sub-leader.
    /// Returns true on success, false if not found / no permission / already cleared.
    /// On success also returns the refreshed deck log.
    /// </summary>
    Task<(bool Ok, DeckLogResult? Log)> DeleteDeckAsync(long viewerId, int messageId, CancellationToken ct = default);

    /// <summary>
    /// Share a replay to guild chat. Stores {"battle_id": N} as the REPLAY message's ReplayPayload.
    /// </summary>
    Task<ChatPostResult> PostReplayAsync(long viewerId, long battleId, CancellationToken ct = default);

    /// <summary>
    /// Fetch the replay payload for a given REPLAY chat message. Returns null if not found / not a replay.
    /// </summary>
    Task<string?> GetReplayDetailAsync(long viewerId, int messageId, CancellationToken ct = default);

    /// <summary>
    /// Return the guild's full deck log — all active (non-deleted) DECK messages grouped by
    /// API-side Format string key. Returns null if caller is not in a guild.
    /// </summary>
    Task<DeckLogResult?> GetDeckLogAsync(long viewerId, CancellationToken ct = default);

    /// <summary>Emit a system-event row. Called by IGuildService at create/join/leave/remove/role-change/description-change.</summary>
    Task EmitSystemEventAsync(int guildId, long actorViewerId, Entities.Guild.GuildChatMessageType type, string? body = null, CancellationToken ct = default);
}
