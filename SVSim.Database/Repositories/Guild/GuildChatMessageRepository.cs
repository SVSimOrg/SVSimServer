using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SVSim.Database.Entities.Guild;
using System.Data;

namespace SVSim.Database.Repositories.Guild;

public sealed class GuildChatMessageRepository : IGuildChatMessageRepository
{
    private readonly SVSimDbContext _db;

    public GuildChatMessageRepository(SVSimDbContext db) { _db = db; }

    /// <summary>
    /// Allocates the next per-guild monotonic MessageId and inserts the row inside a serializable
    /// transaction (Postgres). The unique (GuildId, MessageId) index is the backstop — if a
    /// concurrent insert wins the race we retry once before surfacing the exception.
    /// InMemory provider does not support transactions; callers are single-threaded in tests.
    /// </summary>
    public async Task<GuildChatMessage> AppendAsync(GuildChatMessage msg, CancellationToken ct = default)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            IDbContextTransaction? tx = null;
            try
            {
                // BeginTransactionAsync throws NotSupportedException on the InMemory provider.
                // Catch and fall through — single-threaded tests don't need transaction isolation.
                try
                {
                    tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                }
                catch (InvalidOperationException)
                {
                    // InMemory provider does not support transactions.
                }

                int nextId = await GetMaxMessageIdAsync(msg.GuildId, ct) + 1;
                msg.MessageId = nextId;

                _db.GuildChatMessages.Add(msg);
                await _db.SaveChangesAsync(ct);

                if (tx is not null)
                    await tx.CommitAsync(ct);

                return msg;
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                // Unique violation on (GuildId, MessageId) — another writer raced us.
                // Detach the failed entity and retry once.
                if (tx is not null)
                {
                    try { await tx.RollbackAsync(ct); } catch { /* ignore rollback failure */ }
                }
                _db.Entry(msg).State = EntityState.Detached;
                msg.MessageId = 0; // will be re-assigned on retry
            }
            finally
            {
                if (tx is not null)
                    await tx.DisposeAsync();
            }
        }

        throw new InvalidOperationException(
            $"Failed to allocate a unique MessageId for GuildId={msg.GuildId} after 2 attempts.");
    }

    /// <summary>
    /// Window query for message history.
    /// direction 1 = OLD (walk backwards from start, return ascending),
    /// direction 2 = NEW (walk forwards from start),
    /// direction 3 = BOTH (around start, older half + newer half).
    /// start = 0 means "latest".
    /// Result is always ordered oldest-to-newest.
    /// </summary>
    public async Task<IReadOnlyList<GuildChatMessage>> GetWindowAsync(
        int guildId, int start, int direction, int limit, CancellationToken ct = default)
    {
        IQueryable<GuildChatMessage> baseQ = _db.GuildChatMessages
            .Where(m => m.GuildId == guildId);

        if (start == 0)
        {
            // Latest N messages.
            return await baseQ
                .OrderByDescending(m => m.MessageId)
                .Take(limit)
                .OrderBy(m => m.MessageId)
                .ToListAsync(ct);
        }

        switch (direction)
        {
            case 1: // OLD — messages older than (and including) start
                return await baseQ
                    .Where(m => m.MessageId <= start)
                    .OrderByDescending(m => m.MessageId)
                    .Take(limit)
                    .OrderBy(m => m.MessageId)
                    .ToListAsync(ct);

            case 2: // NEW: strictly newer than start (exclusive)
                return await baseQ
                    .Where(m => m.MessageId > start)
                    .OrderBy(m => m.MessageId)
                    .Take(limit)
                    .ToListAsync(ct);

            case 3: // BOTH — half older, half newer around start
            {
                int half = limit / 2;
                var older = await baseQ
                    .Where(m => m.MessageId <= start)
                    .OrderByDescending(m => m.MessageId)
                    .Take(half)
                    .ToListAsync(ct);
                var newer = await baseQ
                    .Where(m => m.MessageId > start)
                    .OrderBy(m => m.MessageId)
                    .Take(limit - half)
                    .ToListAsync(ct);
                return older.OrderBy(m => m.MessageId).Concat(newer).ToList();
            }

            default:
                return await baseQ
                    .OrderByDescending(m => m.MessageId)
                    .Take(limit)
                    .OrderBy(m => m.MessageId)
                    .ToListAsync(ct);
        }
    }

    public async Task<int> GetMaxMessageIdAsync(int guildId, CancellationToken ct = default)
    {
        var max = await _db.GuildChatMessages
            .Where(m => m.GuildId == guildId)
            .MaxAsync(m => (int?)m.MessageId, ct);
        return max ?? 0;
    }

    public async Task<long?> GetMaxMessageIdSafelyAsync(int guildId, CancellationToken ct = default)
    {
        var v = await GetMaxMessageIdAsync(guildId, ct);
        return v == 0 ? null : (long?)v;
    }

    public async Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default)
    {
        var messages = await _db.GuildChatMessages.Where(m => m.GuildId == guildId).ToListAsync(ct);
        _db.GuildChatMessages.RemoveRange(messages);
        if (messages.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    public Task<GuildChatMessage?> GetByMessageIdAsync(int guildId, int messageId, CancellationToken ct = default)
        => _db.GuildChatMessages
            .FirstOrDefaultAsync(m => m.GuildId == guildId && m.MessageId == messageId, ct);

    public async Task<IReadOnlyList<GuildChatMessage>> GetDeckMessagesAsync(int guildId, CancellationToken ct = default)
        => await _db.GuildChatMessages
            .Where(m => m.GuildId == guildId
                     && m.MessageType == GuildChatMessageType.Deck
                     && m.DeckPayload != null)
            .OrderBy(m => m.MessageId)
            .ToListAsync(ct);

    public async Task<bool> ClearDeckAsync(int guildId, int messageId, long callerViewerId, bool leaderOverride, CancellationToken ct = default)
    {
        var msg = await _db.GuildChatMessages
            .FirstOrDefaultAsync(m => m.GuildId == guildId && m.MessageId == messageId, ct);

        if (msg is null) return false;
        if (!leaderOverride && msg.AuthorViewerId != callerViewerId) return false;
        if (msg.DeckPayload is null) return false;

        msg.DeckPayload = null;
        msg.Body = "";
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
