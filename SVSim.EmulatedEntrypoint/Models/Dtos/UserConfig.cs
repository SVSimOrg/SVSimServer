using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserConfig
{
    [JsonPropertyName("receive_invitation")]
    [Key("receive_invitation")]
    public int ReceiveInvitation { get; set; }
    [JsonPropertyName("receive_invitation_in_battle")]
    [Key("receive_invitation_in_battle")]
    public int ReceiveInvitationInBattle { get; set; }
    [JsonPropertyName("receive_invitation_in_offline")]
    [Key("receive_invitation_in_offline")]
    public int ReceiveInvitationInOffline { get; set; }
    [JsonPropertyName("receive_friend_apply")]
    [Key("receive_friend_apply")]
    public int ReceiveFriendApply { get; set; }
    [JsonPropertyName("is_allow_send_adjust")]
    [Key("is_allow_send_adjust")]
    public int IsAllowSendAdjust { get; set; }
    [JsonPropertyName("is_foil_preferred")]
    [Key("is_foil_preferred")]
    public int IsFoilPreferred { get; set; }
    [JsonPropertyName("is_prize_preferred")]
    [Key("is_prize_preferred")]
    public int IsPrizePreferred { get; set; }
    [JsonPropertyName("is_skip_gacha_effect")]
    [Key("is_skip_gacha_effect")]
    public int IsSkipGachaEffect { get; set; }
}