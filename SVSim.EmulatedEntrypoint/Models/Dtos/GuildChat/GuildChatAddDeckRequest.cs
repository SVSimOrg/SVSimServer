using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Request for POST /guild_chat/add_deck. Shares a deck snapshot to guild chat.</summary>
[MessagePackObject]
public class GuildChatAddDeckRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>API-side Format value of the deck being shared.</summary>
    [JsonPropertyName("deck_format"), Key("deck_format")]
    public int DeckFormat { get; set; }

    /// <summary>Slot number of the deck being shared (within the user's regular deck slots).</summary>
    [JsonPropertyName("deck_no"), Key("deck_no")]
    public int DeckNo { get; set; }
}
