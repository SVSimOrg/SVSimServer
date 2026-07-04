using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class TutorialPresentSeed
{
    [JsonPropertyName("present_id")]       public string PresentId { get; set; } = "";
    [JsonPropertyName("reward_type")]      public int RewardType { get; set; }
    [JsonPropertyName("reward_detail_id")] public long RewardDetailId { get; set; }
    [JsonPropertyName("reward_count")]     public long RewardCount { get; set; }
    [JsonPropertyName("item_type")]        public int? ItemType { get; set; }
    [JsonPropertyName("message")]          public string Message { get; set; } = "";
}
