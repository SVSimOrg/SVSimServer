using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Request for POST /guild_chat/post. Posts a text message or stamp to guild chat.</summary>
[MessagePackObject]
public class GuildChatPostRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>eMessageType: 0=NORMAL (text body), 1=STAMP (stringified stamp id).</summary>
    [JsonPropertyName("type"), Key("type")]
    public int Type { get; set; }

    /// <summary>Text body for NORMAL; stringified stamp id for STAMP.</summary>
    [JsonPropertyName("message"), Key("message")]
    public string Message { get; set; } = "";
}
