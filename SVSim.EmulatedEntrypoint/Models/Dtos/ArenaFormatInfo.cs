using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Current arena season config. Shape derived from 2026-05-23 prod capture
/// (<c>arena_info[0].format_info</c>).
///
/// Wire mixes types: <c>two_pick_type</c> and <c>last_card_pack_set_id</c> are strings
/// (PHP-backend stringification), <c>announce_id</c> is an int, and the times use
/// space-separated "yyyy-MM-dd HH:mm:ss" rather than ISO. Numeric-typed properties use
/// <c>AllowReadingFromString</c> on the controller's JsonSerializerOptions so string-quoted
/// ints deserialize cleanly out of the seeded jsonb.
/// </summary>
[MessagePackObject]
public class ArenaFormatInfo
{
    /// <summary>PickTwoFormat as int (0=None,1=Normal,2=Backdraft,3=Cube,4=Chaos,...).</summary>
    [JsonPropertyName("two_pick_type")]
    [Key("two_pick_type")]
    public int TwoPickType { get; set; }

    [JsonPropertyName("card_pool_name")]
    [Key("card_pool_name")]
    public string CardPoolName { get; set; } = string.Empty;

    [JsonPropertyName("announce_id")]
    [Key("announce_id")]
    public int AnnounceId { get; set; }

    /// <summary>The current card pack set id, e.g. "10029". String on the wire.</summary>
    [JsonPropertyName("last_card_pack_set_id")]
    [Key("last_card_pack_set_id")]
    public string LastCardPackSetId { get; set; } = string.Empty;

    /// <summary>
    /// Wire format is "yyyy-MM-dd HH:mm:ss" (space-separated, prod's PHP convention) — NOT ISO.
    /// Stored as string here so the jsonb passthrough survives byte-for-byte; the client's
    /// DateTime.Parse accepts either format on the receiving side.
    /// </summary>
    [JsonPropertyName("start_time")]
    [Key("start_time")]
    public string StartTime { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public string EndTime { get; set; } = string.Empty;
}
