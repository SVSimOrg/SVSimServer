using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// room_type_in_session on /mypage/index — list of "special" deck-format windows currently active.
/// Consumed by RoomRuleInfo (Wizard/RoomRuleInfo.cs:61) via TryGetValue, but emitted unconditionally
/// per the post-parse-consumer-safe policy.
///
/// Prod-captured shape:
/// <code>{"special_deck_format_list": [{"deck_format":"5","end_time":"2030-06-26 19:59:59"}]}</code>
/// </summary>
[MessagePackObject]
public class RoomTypeInSession
{
    [JsonPropertyName("special_deck_format_list")]
    [Key("special_deck_format_list")]
    public List<SpecialDeckFormat> SpecialDeckFormatList { get; set; } = new();
}
