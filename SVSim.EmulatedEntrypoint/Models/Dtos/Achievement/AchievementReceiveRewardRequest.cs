using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Achievement;

[MessagePackObject]
public class AchievementReceiveRewardRequest : BaseRequest
{
    [Key("achievement_type")]
    [JsonPropertyName("achievement_type")]
    public int AchievementType { get; set; }

    [Key("level")]
    [JsonPropertyName("level")]
    public int Level { get; set; }
}
