using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// Single-field `{ "is_display_badge": bool }` wrapper. The badge-poll context
/// of <c>MyPageNotifications.ParseBadgeInfos</c> (called from StoryFinishTask,
/// QuestFinishTask, RecoveryTask, OpenRoomBattleGetRecoveryParamTask) reads
/// only this one field from each of <c>quest</c>, <c>story_notification</c>,
/// and <c>basic_puzzle</c>, so all three positions share this shape.
///
/// The mypage-index versions of <c>quest</c> and <c>story_notification</c> have
/// richer shapes (<see cref="SVSim.EmulatedEntrypoint.Models.Dtos.Quest"/>,
/// <see cref="SVSim.EmulatedEntrypoint.Models.Dtos.StoryNotification"/>) since
/// the home-screen UI reads additional fields off them.
/// </summary>
[MessagePackObject]
public class BadgeFlag
{
    [JsonPropertyName("is_display_badge")]
    [Key("is_display_badge")]
    public bool IsDisplayBadge { get; set; }
}
