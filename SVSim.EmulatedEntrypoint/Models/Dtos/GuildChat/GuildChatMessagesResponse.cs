using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>Response for POST /guild_chat/messages.</summary>
[MessagePackObject]
public class GuildChatMessagesResponse
{
    /// <summary>Card ids currently disabled for maintenance — client refreshes its global list.</summary>
    [JsonPropertyName("maintenance_card_list"), Key("maintenance_card_list"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<int> MaintenanceCardList { get; set; } = new();

    /// <summary>Users referenced by messages in this batch — deduplicated catalog.</summary>
    [JsonPropertyName("users"), Key("users"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<ChatUserDto> Users { get; set; } = new();

    /// <summary>Messages ordered oldest-to-newest.</summary>
    [JsonPropertyName("chat_message"), Key("chat_message"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<ChatMessageDto> ChatMessage { get; set; } = new();

    /// <summary>Server-driven polling interval in seconds. Client uses this for the next poll.</summary>
    [JsonPropertyName("wait_interval"), Key("wait_interval"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int WaitInterval { get; set; }
}
