using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Request for POST /guild_chat/add_replay. Shares a battle replay to guild chat.</summary>
[MessagePackObject]
public class GuildChatAddReplayRequest
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "";

    [JsonPropertyName("steam_id"), Key("steam_id")]
    public ulong SteamId { get; set; }

    [JsonPropertyName("steam_session_ticket"), Key("steam_session_ticket")]
    public string SteamSessionTicket { get; set; } = "";

    /// <summary>Battle id of the replay being shared. long per spec.</summary>
    [JsonPropertyName("battle_id"), Key("battle_id")]
    public long BattleId { get; set; }
}
