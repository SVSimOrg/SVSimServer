using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One entry under /mypage/index data.room_type_in_session.special_deck_format_list. Consumed by
/// RoomRuleInfo ctor (Wizard/RoomRuleInfo.cs:61-70) which is TryGetValue-guarded but the per-entry
/// fields are accessed unconditionally inside the guard.
///
/// Prod-captured shape: <c>{"deck_format":"5","end_time":"2030-06-26 19:59:59"}</c>.
/// </summary>
[MessagePackObject]
public class SpecialDeckFormat
{
    /// <summary>Wire is string per prod's PHP convention (despite looking numeric like "5").</summary>
    [JsonPropertyName("deck_format")]
    [Key("deck_format")]
    public string DeckFormat { get; set; } = string.Empty;

    /// <summary>"yyyy-MM-dd HH:mm:ss" wire format.</summary>
    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public string EndTime { get; set; } = string.Empty;
}
