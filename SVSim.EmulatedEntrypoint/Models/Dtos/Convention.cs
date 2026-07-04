using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Convention/offline-event participation block returned by /mypage/index.
/// Client reads is_join_tournament, recent_start_date (null-checked, optional),
/// and is_admin_watch_user. See MyPageTask.cs:58-63.
/// </summary>
[MessagePackObject]
public class Convention
{
    [JsonPropertyName("is_join_tournament")]
    [Key("is_join_tournament")]
    public bool IsJoinTournament { get; set; }

    /// <summary>
    /// ISO datetime, or null when no recent tournament. Client does
    /// `if (jsonData["convention"]["recent_start_date"] != null)` (MyPageTask.cs:59) —
    /// the key must be PRESENT (LitJson throws KeyNotFoundException on missing key);
    /// the null check exists to detect "no recent tournament", not "field absent".
    /// Override the global WhenWritingNull so the explicit null reaches the wire,
    /// matching prod's `"recent_start_date":null` in the convention block.
    /// </summary>
    [JsonPropertyName("recent_start_date")]
    [Key("recent_start_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? RecentStartDate { get; set; }

    [JsonPropertyName("is_admin_watch_user")]
    [Key("is_admin_watch_user")]
    public bool IsAdminWatchUser { get; set; }
}
