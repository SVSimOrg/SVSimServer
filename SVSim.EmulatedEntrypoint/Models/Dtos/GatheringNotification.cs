using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// gathering_notification on /mypage/refresh — slim "matching established?" notification flag for
/// gathering events. Single field carrying either an empty string (no match) or the localized
/// "matching established" message (active match).
///
/// **Distinct from <see cref="GatheringInfo"/>**, which is what /mypage/index emits under the
/// <c>gathering_info</c> key — that DTO carries the viewer's full event participation state
/// (has_invite / is_entry). They share a topic ("gathering events") but solve different problems
/// and live at different wire keys; don't conflate them.
///
/// Consumed unconditionally at <c>MyPageRefreshTask.cs:31</c>:
/// <c>jsonData["data"]["gathering_notification"]["matching_established_message"].ToString()</c>.
/// </summary>
[MessagePackObject]
public class GatheringNotification
{
    /// <summary>Empty string when no match — correct for fresh viewers and idle states. Prod sends "".</summary>
    [JsonPropertyName("matching_established_message")]
    [Key("matching_established_message")]
    public string MatchingEstablishedMessage { get; set; } = string.Empty;
}
