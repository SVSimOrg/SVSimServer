using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One entry of <c>reward_list</c> on /pack/open (and other grant-emitting endpoints).
/// Client at <c>PlayerStaticData.UpdateHaveUserGoodsNumByJsonData</c> reads these and writes
/// <c>UserRupyCount = reward_num</c>, <c>UserCrystalCount = reward_num</c>, etc. —
/// <b>reward_num is the NEW POST-STATE TOTAL, not a delta</b>. Without these entries the
/// client's cached currency/collection counts stay stale until a full refresh (mypage, restart).
///
/// reward_type values are <c>Wizard.UserGoods.Type</c>: 1=RedEther, 2=Crystal, 4=Item, 5=Card,
/// 6=Sleeve, 7=Emblem, 8=Degree, 9=Rupy, 10=Skin, 11=SpotCard, 12=SpotCardPoint, etc.
/// reward_id is 0 for non-instanced goods (Rupy, Crystal, RedEther) and the entity id for cards.
/// </summary>
[MessagePackObject]
public class RewardListEntry
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public int RewardType { get; set; }

    [JsonPropertyName("reward_id")]
    [Key("reward_id")]
    public long RewardId { get; set; }

    [JsonPropertyName("reward_num")]
    [Key("reward_num")]
    public int RewardNum { get; set; }
}
