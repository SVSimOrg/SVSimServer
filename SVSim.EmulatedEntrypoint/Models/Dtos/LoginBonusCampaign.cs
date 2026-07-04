using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// One login-bonus campaign panel. Used for normal, each campaign array entry, and (when
/// populated) total. <c>campaign_id</c> and <c>img</c> ship as strings on the wire. Client
/// consumers: NormalData (reads name, now_count, is_next_reward, reward[]),
/// SpecialData (also reads img, is_one_day_multi_rewards — start_date/end_date are not
/// modeled yet and ship absent until a real campaign capture lands),
/// ContinuousData (reads only reward[]).
/// </summary>
[MessagePackObject]
public class LoginBonusCampaign
{
    [JsonPropertyName("name")] [Key("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("campaign_id")] [Key("campaign_id")]
    public string CampaignId { get; set; } = "0";

    [JsonPropertyName("img")] [Key("img")]
    public string Img { get; set; } = "0";

    [JsonPropertyName("now_count")] [Key("now_count")]
    public int NowCount { get; set; }

    [JsonPropertyName("is_next_reward")] [Key("is_next_reward")]
    public bool IsNextReward { get; set; }

    [JsonPropertyName("reward")] [Key("reward")]
    public List<LoginBonusReward> Rewards { get; set; } = new();

    /// <summary>
    /// SpecialData-only on the client (SpecialData.cs:50). Server emits it on every panel
    /// for byte-faithful replay; client ignores it on Normal/Total paths.
    /// </summary>
    [JsonPropertyName("is_one_day_multi_rewards")] [Key("is_one_day_multi_rewards")]
    public bool IsOneDayMultiRewards { get; set; }
}
