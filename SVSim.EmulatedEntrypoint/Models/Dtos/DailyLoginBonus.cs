using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Wire shape: { normal?, total?, campaign? (array) }. Client parser at
/// LoadDetail.cs:553 reads exactly these three keys. <c>normal</c> presence drives
/// IsExistLoginBonusData() / popup eligibility.
/// </summary>
[MessagePackObject]
public class DailyLoginBonus
{
    [JsonPropertyName("normal")] [Key("normal")]
    public LoginBonusCampaign? Normal { get; set; }

    [JsonPropertyName("total")] [Key("total")]
    public LoginBonusCampaign? Total { get; set; }

    /// <summary>
    /// Array of campaign panels (LoadDetail.cs:583 iterates jsonData3[i]). Empty list
    /// when no specials are active — prod sends [] not null in that case.
    /// </summary>
    [JsonPropertyName("campaign")] [Key("campaign")]
    public List<LoginBonusCampaign> Campaign { get; set; } = new();
}
