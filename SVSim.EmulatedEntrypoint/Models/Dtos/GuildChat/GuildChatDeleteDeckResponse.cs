using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.GuildChat;

/// <summary>
/// Response for POST /guild_chat/delete_deck.
/// Returns the refreshed deck_log after deletion — same shape as /guild_chat/deck_log.
/// </summary>
[MessagePackObject]
public class GuildChatDeleteDeckResponse
{
    [JsonPropertyName("maintenance_card_list"), Key("maintenance_card_list"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<int> MaintenanceCardList { get; set; } = new();

    /// <summary>
    /// Refreshed deck log keyed by API-side Format int (stringified, e.g. "1", "2", "3").
    /// Crossover ("4") / MyRotation ("5") included only when non-empty.
    /// </summary>
    [JsonPropertyName("deck_log"), Key("deck_log"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, List<DeckLogDataDto>> DeckLog { get; set; } = new();
}
