using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// Flat 4-bool form of <c>shop_notification</c> returned by the badge-poll
/// endpoints (StoryFinish, QuestFinish, Recovery, OpenRoomBattleGetRecoveryParam).
/// Each bool drives the corresponding shop tab's footer badge via
/// <c>ShopNotification.SetShopBadgeEnable</c> (Wizard/ShopNotification.cs:63),
/// which calls <c>.ToBoolean()</c> on each directly.
///
/// Distinct from <see cref="SVSim.EmulatedEntrypoint.Models.Dtos.ShopNotification"/>,
/// which is the richer mypage-index shape (each sub-key holds a detail object
/// instead of a bool, for the home-screen's animated shop appeals).
/// </summary>
[MessagePackObject]
public class ShopNotificationBadges
{
    [JsonPropertyName("card_pack")]
    [Key("card_pack")]
    public bool CardPack { get; set; }

    [JsonPropertyName("build_deck")]
    [Key("build_deck")]
    public bool BuildDeck { get; set; }

    [JsonPropertyName("sleeve")]
    [Key("sleeve")]
    public bool Sleeve { get; set; }

    [JsonPropertyName("leader_skin")]
    [Key("leader_skin")]
    public bool LeaderSkin { get; set; }
}
