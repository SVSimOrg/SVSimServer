using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Request for POST /guild_chat/replay_detail. Fetches full replay payload for a shared replay.</summary>
[MessagePackObject]
public class GuildChatReplayDetailRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Message id of the REPLAY chat message.</summary>
    [JsonPropertyName("message_id"), Key("message_id")]
    public long MessageId { get; set; }
}
