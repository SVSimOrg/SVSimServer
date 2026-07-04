using MessagePack;
using System.Text.Json;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

/// <summary>
/// Wire shape for a single chat message in /guild_chat/messages `chat_message[]`
/// and /gathering_chat/messages. Matches guild_chat-messages.md ChatMessage interface.
///
/// deck / replay / room are JsonElement? so arbitrary payloads can pass through in Phase 5
/// without premature typing (TODO task-17: replace with typed DTOs when replay subsystem is documented).
/// </summary>
[MessagePackObject]
public class ChatMessageDto
{
    [JsonPropertyName("viewer_id"), Key("viewer_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long ViewerId { get; set; }

    [JsonPropertyName("message_id"), Key("message_id"), JsonConverter(typeof(StringifiedLongConverter))]
    public long MessageId { get; set; }

    /// <summary>eMessageType: 0=NORMAL 1=STAMP 2=DECK 3=JOIN 4=LEAVE 5=REPLAY etc.</summary>
    [JsonPropertyName("message_type"), Key("message_type"), JsonConverter(typeof(StringifiedIntConverter))]
    public int MessageType { get; set; }

    [JsonPropertyName("create_time"), Key("create_time"), JsonConverter(typeof(StringifiedLongConverter))]
    public long CreateTime { get; set; }

    /// <summary>Text body for NORMAL, stringified stamp id for STAMP, etc.</summary>
    [JsonPropertyName("message"), Key("message")]
    public string Message { get; set; } = "";

    /// <summary>Present when message_type = DECK (2). Inline DeckLogData payload.</summary>
    [JsonPropertyName("deck"), Key("deck")]
    public JsonElement? Deck { get; set; }

    /// <summary>Present when message_type = REPLAY (5). Inline ReplayInfoItem payload.</summary>
    [JsonPropertyName("replay"), Key("replay")]
    public JsonElement? Replay { get; set; }

    /// <summary>Present when message_type = ROOM_MATCH (10). Inline room-invite payload.</summary>
    [JsonPropertyName("room"), Key("room")]
    public JsonElement? Room { get; set; }

    /// <summary>GATHERING_TOURNAMENT_ROOM extras — only meaningful for /gathering_chat.</summary>
    [JsonPropertyName("viewer_id1"), Key("viewer_id1")]
    public long? ViewerId1 { get; set; }

    /// <summary>GATHERING_TOURNAMENT_ROOM extras — only meaningful for /gathering_chat.</summary>
    [JsonPropertyName("viewer_id2"), Key("viewer_id2")]
    public long? ViewerId2 { get; set; }
}
