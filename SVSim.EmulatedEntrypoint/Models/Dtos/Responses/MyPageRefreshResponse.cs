using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

/// <summary>
/// /mypage/refresh response — a slim notification-delta payload, NOT a full state refresh.
/// Prod sends exactly 3 top-level keys, all of which the client reads unconditionally:
///
/// <list type="bullet">
///   <item><c>friend_battle_invite_count</c> — int, viewer's room-invite count
///         (consumed at <c>MyPageRefreshTask.cs:29</c>).</item>
///   <item><c>shop_notification</c> — same nested shape as /mypage/index's shop_notification.
///         The side-effect call <c>ShopNotification.SetShopNotification</c> unconditionally indexes
///         all four sub-keys (card_pack / build_deck / sleeve / leader_skin), already handled by
///         our <see cref="ShopNotification"/> DTO's field initializers.</item>
///   <item><c>gathering_notification</c> — new shape distinct from /mypage/index's gathering_info.
///         Carries only the matching-established message string.</item>
/// </list>
///
/// All three fields are required-present per the new "anything prod emits, we emit" methodology
/// — even though the third call site looks tolerant, omitting the key would throw
/// KeyNotFoundException at LitJson's indexer.
/// </summary>
[MessagePackObject]
public class MyPageRefreshResponse
{
    [JsonPropertyName("friend_battle_invite_count")]
    [Key("friend_battle_invite_count")]
    public int FriendBattleInviteCount { get; set; }

    [JsonPropertyName("shop_notification")]
    [Key("shop_notification")]
    public ShopNotification ShopNotification { get; set; } = new();

    [JsonPropertyName("gathering_notification")]
    [Key("gathering_notification")]
    public GatheringNotification GatheringNotification { get; set; } = new();
}
