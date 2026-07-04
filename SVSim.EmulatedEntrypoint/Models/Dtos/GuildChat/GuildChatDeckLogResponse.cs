using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>
/// Response for POST /guild_chat/deck_log.
/// deck_log keys are stringified API-side Format ints (e.g. "1", "2", "3").
/// Rotation / Unlimited / PreRotation buckets are always present.
/// Crossover / MyRotation keys are included only when non-empty (client uses TryGetValue).
/// </summary>
[MessagePackObject]
public class GuildChatDeckLogResponse
{
    [JsonPropertyName("maintenance_card_list"), Key("maintenance_card_list"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<int> MaintenanceCardList { get; set; } = new();

    /// <summary>
    /// Decks shared in chat, bucketed by API-side Format value (stringified int keys).
    /// Keys present: "1" (Rotation), "2" (Unlimited), "3" (PreRotation); optionally "4" (Crossover), "5" (MyRotation).
    /// </summary>
    [JsonPropertyName("deck_log"), Key("deck_log"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, List<DeckLogDataDto>> DeckLog { get; set; } = new();
}
