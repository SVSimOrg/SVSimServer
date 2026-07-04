using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Enums;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services.Guild;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;
using SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>/guild_chat/* — 7 endpoints. See docs/api-spec/endpoints/post-login/guild_chat-*.md.</summary>
[Route("guild_chat")]
public sealed class GuildChatController : SVSimController
{
    private readonly IGuildChatService _chat;
    private readonly IViewerRepository _viewers;

    public GuildChatController(IGuildChatService chat, IViewerRepository viewers)
    {
        _chat    = chat;
        _viewers = viewers;
    }

    [HttpPost("messages")]
    public async Task<ActionResult<GuildChatMessagesResponse>> Messages(
        [FromBody] GuildChatMessagesRequest req,
        CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var window = await _chat.GetWindowAsync(
            viewerId,
            (int)req.StartMessageId,
            req.Direction,
            req.WaitInterval,
            ct);

        // Build chat_message list
        var chatMessages = window.Messages.Select(m => new ChatMessageDto
        {
            ViewerId    = m.AuthorViewerId,
            MessageId   = m.MessageId,
            MessageType = (int)m.MessageType,
            CreateTime  = new DateTimeOffset(m.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds(),
            Message     = m.Body,
            Deck        = m.DeckPayload    is not null ? ParseJsonElementOrNull(m.DeckPayload)    : null,
            Replay      = m.ReplayPayload  is not null ? ParseJsonElementOrNull(m.ReplayPayload)  : null,
            Room        = m.RoomPayload    is not null ? ParseJsonElementOrNull(m.RoomPayload)    : null,
        }).ToList();

        // Build deduplicated users[] catalog from message authors
        var authorIds = window.Messages
            .Select(m => m.AuthorViewerId)
            .Distinct()
            .ToList();

        var profiles = await _viewers.LoadChatProfilesAsync(authorIds, ct);

        var users = authorIds.Select(vid =>
        {
            profiles.TryGetValue(vid, out var p);
            return new ChatUserDto
            {
                ViewerId    = vid,
                Name        = p?.Name        ?? "",
                EmblemId    = p?.EmblemId    ?? 100_000_000L,
                CountryCode = p?.CountryCode ?? "",
                Rank        = p?.Rank        ?? 1,
                DegreeId    = p?.DegreeId    ?? 0,
            };
        }).ToList();

        return new GuildChatMessagesResponse
        {
            MaintenanceCardList = new(),
            Users               = users,
            ChatMessage         = chatMessages,
            WaitInterval        = window.WaitIntervalSeconds,
        };
    }

    [HttpPost("post")]
    public async Task<ActionResult<GuildChatPostResponse>> Post([FromBody] GuildChatPostRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var result = await _chat.PostTextOrStampAsync(viewerId, req.Type, req.Message, ct);
        if (!result.Ok) return Ok(new { result_code = 2 });
        return new GuildChatPostResponse();
    }

    [HttpPost("add_deck")]
    public async Task<ActionResult<EmptyResponse>> AddDeck([FromBody] GuildChatAddDeckRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        Format format;
        try { format = FormatExtensions.FromApi(req.DeckFormat); }
        catch (ArgumentOutOfRangeException) { return Ok(new { result_code = 2 }); }

        var result = await _chat.PostDeckAsync(viewerId, format, req.DeckFormat, req.DeckNo, ct);
        if (!result.Ok) return Ok(new { result_code = 2 });
        return new EmptyResponse();
    }

    [HttpPost("delete_deck")]
    public async Task<ActionResult<GuildChatDeleteDeckResponse>> DeleteDeck([FromBody] GuildChatDeleteDeckRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var (ok, log) = await _chat.DeleteDeckAsync(viewerId, (int)req.MessageId, ct);
        if (!ok || log is null) return Ok(new { result_code = 2 });

        return new GuildChatDeleteDeckResponse
        {
            MaintenanceCardList = new(),
            DeckLog             = BuildDeckLogDict(log, viewerId),
        };
    }

    [HttpPost("add_replay")]
    public async Task<ActionResult<EmptyResponse>> AddReplay([FromBody] GuildChatAddReplayRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var result = await _chat.PostReplayAsync(viewerId, req.BattleId, ct);
        if (!result.Ok) return Ok(new { result_code = 2 });
        return new EmptyResponse();
    }

    [HttpPost("replay_detail")]
    public async Task<ActionResult> ReplayDetail([FromBody] GuildChatReplayDetailRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var payloadJson = await _chat.GetReplayDetailAsync(viewerId, (int)req.MessageId, ct);
        if (payloadJson is null) return Ok(new { result_code = 2 });

        // ReplayDetailInfo(data) accesses data["battleId"], data["seed"], data["vid1"], etc.
        // directly without Keys.Contains guards — returning a wrapper object crashes the client.
        // The stored ReplayPayload IS the full flat battle object; emit its fields directly as
        // the data payload so data["battleId"].ToLong() etc. resolve correctly.
        var replayElement = ParseJsonElementOrNull(payloadJson);
        return replayElement.HasValue ? Ok(replayElement.Value) : Ok(new { result_code = 2 });
    }

    [HttpPost("deck_log")]
    public async Task<ActionResult<GuildChatDeckLogResponse>> DeckLog([FromBody] GuildChatDeckLogRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var log = await _chat.GetDeckLogAsync(viewerId, ct);
        if (log is null) return Ok(new { result_code = 2 });

        return new GuildChatDeckLogResponse
        {
            MaintenanceCardList = new(),
            DeckLog             = BuildDeckLogDict(log, viewerId),
        };
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Converts a DeckLogResult into the wire deck_log dictionary.
    /// Keys "1", "2", "3" (Rotation/Unlimited/PreRotation) are always present.
    /// Keys "4" (Crossover) and "5" (MyRotation) are only included when non-empty.
    /// delete_permission_exists is true when the calling viewer is the author (leader override
    /// is handled at the delete endpoint — here it's purely author-based for the display flag).
    /// </summary>
    private static Dictionary<string, List<DeckLogDataDto>> BuildDeckLogDict(DeckLogResult log, long callerViewerId)
    {
        // Always-present buckets.
        var dict = new Dictionary<string, List<DeckLogDataDto>>
        {
            ["1"] = new(), // Rotation
            ["2"] = new(), // Unlimited
            ["3"] = new(), // PreRotation
        };

        foreach (var entry in log.Entries)
        {
            DeckLogDataDto? dto = BuildDeckLogDataDto(entry, callerViewerId);
            if (dto is null) continue;

            string key = dto.DeckFormat.ToApi().ToString();

            if (!dict.TryGetValue(key, out var bucket))
            {
                // Optional bucket (Crossover=4, MyRotation=5) — create on first use.
                bucket = new List<DeckLogDataDto>();
                dict[key] = bucket;
            }
            bucket.Add(dto);
        }

        return dict;
    }

    /// <summary>
    /// Deserializes a stored DeckPayload JSON into a DeckLogDataDto.
    /// Returns null if the JSON is malformed or missing required fields.
    /// </summary>
    private static DeckLogDataDto? BuildDeckLogDataDto(DeckLogEntry entry, long callerViewerId)
    {
        try
        {
            using var doc = JsonDocument.Parse(entry.PayloadJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("deck_format", out var fmtEl)) return null;
            int deckFormatApi = fmtEl.GetInt32();

            Format format;
            try { format = FormatExtensions.FromApi(deckFormatApi); }
            catch (ArgumentOutOfRangeException) { return null; }

            var cardIdArray = new List<long>();
            if (root.TryGetProperty("card_id_array", out var arrEl))
            {
                foreach (var el in arrEl.EnumerateArray())
                    cardIdArray.Add(el.GetInt64());
            }

            return new DeckLogDataDto
            {
                DeckFormat             = format,
                MessageId              = entry.MessageId,
                DeletePermissionExists = entry.AuthorViewerId == callerViewerId,
                DeckNo                 = root.TryGetProperty("deck_no",  out var dno) ? dno.GetInt32()     : 0,
                DeckName               = root.TryGetProperty("deck_name", out var dn) ? dn.GetString() ?? "" : "",
                ClassId                = root.TryGetProperty("class_id", out var ci)  ? ci.GetInt32()     : 0,
                SleeveId               = root.TryGetProperty("sleeve_id", out var si) ? si.GetInt64()     : 3_000_011L,
                LeaderSkinId           = root.TryGetProperty("leader_skin_id", out var ls) ? ls.GetInt32() : 0,
                CardIdArray            = cardIdArray,
            };
        }
        catch
        {
            return null;
        }
    }

    private static JsonElement? ParseJsonElementOrNull(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }
}
