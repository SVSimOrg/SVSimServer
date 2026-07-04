using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public class ArenaTwoPickRewardSeed
{
    [JsonPropertyName("win_count")]    public int WinCount { get; set; }
    [JsonPropertyName("reward_group")] public int RewardGroup { get; set; }
    [JsonPropertyName("weight")]       public int Weight { get; set; } = 1;
    [JsonPropertyName("reward_type")]  public int RewardType { get; set; }
    [JsonPropertyName("reward_id")]    public long RewardId { get; set; }
    [JsonPropertyName("reward_num")]   public int RewardNum { get; set; }
}
