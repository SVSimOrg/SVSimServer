using System.Text.Json;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Enums;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Guild;
using SVSim.Database.Services;

namespace SVSim.Database.Services.Guild;

public sealed class GuildChatService : IGuildChatService
{
    private readonly IGuildMemberRepository _members;
    private readonly IGuildChatMessageRepository _msgs;
    private readonly IGameConfigService _cfg;
    private readonly IDeckRepository _decks;

    public GuildChatService(
        IGuildMemberRepository members,
        IGuildChatMessageRepository msgs,
        IGameConfigService cfg,
        IDeckRepository decks)
    {
        _members = members;
        _msgs    = msgs;
        _cfg     = cfg;
        _decks   = decks;
    }

    // ── System-event emission ────────────────────────────────────────────────

    public async Task EmitSystemEventAsync(
        int guildId,
        long actorViewerId,
        GuildChatMessageType type,
        string? body = null,
        CancellationToken ct = default)
    {
        var msg = new GuildChatMessage
        {
            GuildId         = guildId,
            AuthorViewerId  = actorViewerId,
            MessageType     = type,
            Body            = body ?? "",
            CreatedAt       = DateTime.UtcNow,
        };
        await _msgs.AppendAsync(msg, ct);
    }

    // ── Window query ─────────────────────────────────────────────────────────

    public async Task<ChatWindow> GetWindowAsync(
        long viewerId,
        int startMessageId,
        int direction,
        int waitIntervalHint,
        CancellationToken ct = default)
    {
        var membership = await _members.GetMembershipAsync(viewerId, ct);
        if (membership is null)
        {
            var idleInterval = _cfg.Get<GuildConfig>().ChatPollIdleSeconds;
            return new(Array.Empty<GuildChatMessage>(), idleInterval);
        }

        const int limit = 50;
        var msgs = await _msgs.GetWindowAsync(membership.GuildId, startMessageId, direction, limit, ct);

        var cfg = _cfg.Get<GuildConfig>();
        int wait = msgs.Count == 0 ? cfg.ChatPollIdleSeconds : cfg.ChatPollActiveSeconds;
        return new(msgs, wait);
    }

    // ── Post text / stamp ────────────────────────────────────────────────────

    public async Task<ChatPostResult> PostTextOrStampAsync(long viewerId, int type, string message, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(false, null);

        var cfg = _cfg.Get<GuildConfig>();
        if (type == 1)
        {
            if (!int.TryParse(message, out var stampId) || !cfg.UsableStampList.Contains(stampId))
                return new(false, null);
        }
        else if (type != 0)
        {
            // Only NORMAL (0) and STAMP (1) are valid via this endpoint.
            return new(false, null);
        }

        var now    = DateTime.UtcNow;
        var nextId = await _msgs.GetMaxMessageIdAsync(m.GuildId, ct) + 1;
        var stored = await _msgs.AppendAsync(new GuildChatMessage
        {
            GuildId        = m.GuildId,
            MessageId      = nextId,
            AuthorViewerId = viewerId,
            MessageType    = type == 1 ? GuildChatMessageType.Stamp : GuildChatMessageType.Normal,
            Body           = message,
            CreatedAt      = now,
        }, ct);
        return new(true, stored.MessageId);
    }

    // ── Deck attachment ──────────────────────────────────────────────────────

    public async Task<ChatPostResult> PostDeckAsync(long viewerId, Format format, int deckFormatApi, int deckNo, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(false, null);

        var deck = await _decks.GetDeck(viewerId, format, deckNo);
        if (deck is null) return new(false, null);

        // Build the deck payload JSON that the client expects to parse via DeckLogData / DeckData.Initialize.
        // Fields: deck_format (API int), deck_no, deck_name, class_id, sleeve_id, leader_skin_id, card_id_array.
        var cardIdArray = deck.Cards
            .SelectMany(c => Enumerable.Repeat(c.Card.Id, c.Count))
            .ToList();

        var payload = new
        {
            deck_format     = deckFormatApi,
            deck_no         = deck.Number,
            deck_name       = deck.Name,
            class_id        = deck.Class.Id,
            sleeve_id       = (long)deck.Sleeve.Id,
            leader_skin_id  = deck.LeaderSkin.Id,
            card_id_array   = cardIdArray,
        };
        var payloadJson = JsonSerializer.Serialize(payload);

        var stored = await _msgs.AppendAsync(new GuildChatMessage
        {
            GuildId        = m.GuildId,
            AuthorViewerId = viewerId,
            MessageType    = GuildChatMessageType.Deck,
            Body           = "",
            DeckPayload    = payloadJson,
            CreatedAt      = DateTime.UtcNow,
        }, ct);
        return new(true, stored.MessageId);
    }

    public async Task<(bool Ok, DeckLogResult? Log)> DeleteDeckAsync(long viewerId, int messageId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return (false, null);

        bool isLeader = m.Role is GuildRole.Leader or GuildRole.SubLeader;
        bool cleared  = await _msgs.ClearDeckAsync(m.GuildId, messageId, viewerId, isLeader, ct);
        if (!cleared) return (false, null);

        var log = await BuildDeckLogAsync(m.GuildId, ct);
        return (true, log);
    }

    // ── Replay attachment ────────────────────────────────────────────────────

    public async Task<ChatPostResult> PostReplayAsync(long viewerId, long battleId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(false, null);

        // Store minimal payload; replay subsystem detail is TODO(post-merge).
        var payloadJson = JsonSerializer.Serialize(new { battle_id = battleId });

        var stored = await _msgs.AppendAsync(new GuildChatMessage
        {
            GuildId        = m.GuildId,
            AuthorViewerId = viewerId,
            MessageType    = GuildChatMessageType.Replay,
            Body           = "",
            ReplayPayload  = payloadJson,
            CreatedAt      = DateTime.UtcNow,
        }, ct);
        return new(true, stored.MessageId);
    }

    public async Task<string?> GetReplayDetailAsync(long viewerId, int messageId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return null;

        var msg = await _msgs.GetByMessageIdAsync(m.GuildId, messageId, ct);
        return msg?.ReplayPayload;
    }

    // ── Deck log ─────────────────────────────────────────────────────────────

    public async Task<DeckLogResult?> GetDeckLogAsync(long viewerId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return null;
        return await BuildDeckLogAsync(m.GuildId, ct);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<DeckLogResult> BuildDeckLogAsync(int guildId, CancellationToken ct)
    {
        var deckMessages = await _msgs.GetDeckMessagesAsync(guildId, ct);
        var entries = deckMessages
            .Where(msg => msg.DeckPayload is not null)
            .Select(msg => new DeckLogEntry(msg.MessageId, msg.AuthorViewerId, msg.DeckPayload!))
            .ToList();
        return new DeckLogResult(entries);
    }
}
