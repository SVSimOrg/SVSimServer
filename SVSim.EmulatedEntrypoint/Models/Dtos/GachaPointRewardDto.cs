using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One row of <c>gacha_point_rewards[]</c>. The client groups by <see cref="ClassId"/>
/// (CardBasePrm.ClanType, stringified on the wire) inside GachaPointExchangeInfoTask.Parse.
/// </summary>
[MessagePackObject]
public class GachaPointRewardDto
{
    /// <summary>Stringified on the wire (e.g. "0", "1"). CardBasePrm.ClanType value.</summary>
    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public string ClassId { get; set; } = "0";

    [JsonPropertyName("card_id")]
    [Key("card_id")]
    public long CardId { get; set; }

    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<GachaPointRewardDetailEntry> RewardList { get; set; } = new();

    [JsonPropertyName("is_received")]
    [Key("is_received")]
    public bool IsReceived { get; set; }

    [JsonPropertyName("is_display_prize")]
    [Key("is_display_prize")]
    public bool IsDisplayPrize { get; set; }
}
