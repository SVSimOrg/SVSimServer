using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// gathering_info on /mypage/index — multiplayer-event participation state. Consumed by
/// GatheringMyPageInfo ctor (TryGetValue-guarded) but emitted unconditionally to match prod and
/// to keep post-parse UI consumers from reading nulls.
/// </summary>
[MessagePackObject]
public class GatheringInfo
{
    [JsonPropertyName("has_invite")]
    [Key("has_invite")]
    public int HasInvite { get; set; }

    /// <summary>Whether this viewer has entered the current gathering event. Per-viewer state — currently always 0.</summary>
    [JsonPropertyName("is_entry")]
    [Key("is_entry")]
    public int IsEntry { get; set; }
}
