using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Request for POST /guild_chat/messages. Polling endpoint for guild chat.</summary>
[MessagePackObject]
public class GuildChatMessagesRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Anchor message id. 0 = latest log head.</summary>
    [JsonPropertyName("start_message_id"), Key("start_message_id")]
    public long StartMessageId { get; set; }

    /// <summary>Chat.eRequestDirection: 1=OLD 2=NEW 3=BOTH.</summary>
    [JsonPropertyName("direction"), Key("direction")]
    public int Direction { get; set; }

    /// <summary>Client's current polling interval in seconds.</summary>
    [JsonPropertyName("wait_interval"), Key("wait_interval")]
    public int WaitInterval { get; set; }
}
