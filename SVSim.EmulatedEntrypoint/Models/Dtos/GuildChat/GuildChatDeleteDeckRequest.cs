using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Request for POST /guild_chat/delete_deck. Deletes a previously-shared deck from the chat archive.</summary>
[MessagePackObject]
public class GuildChatDeleteDeckRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>API-side Format of the deck being deleted.</summary>
    [JsonPropertyName("deck_format"), Key("deck_format")]
    public int DeckFormat { get; set; }

    /// <summary>Message id of the chat message that originally shared the deck.</summary>
    [JsonPropertyName("message_id"), Key("message_id")]
    public long MessageId { get; set; }
}
