using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

[MessagePackObject]
public class GameStartRequest : BaseRequest
{
    [JsonPropertyName("app_type")]
    [Key("app_type")]
    public int AppType { get; set; }
    [JsonPropertyName("campaign_data")]
    [Key("campaign_data")]
    public string CampaignData { get; set; }
    [JsonPropertyName("campaign_sign")]
    [Key("campaign_sign")]
    public string CampaignSign { get; set; }
    [JsonPropertyName("campaign_user")]
    [Key("campaign_user")]
    public int CampaignUser { get; set; }
}