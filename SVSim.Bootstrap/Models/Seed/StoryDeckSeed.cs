using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class StoryDeckSeed
{
    [JsonPropertyName("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = "build";
    [JsonPropertyName("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("deck_name")] public string DeckName { get; set; } = "";
    [JsonPropertyName("sleeve_id")] public int SleeveId { get; set; }
    [JsonPropertyName("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("is_recommend")] public int IsRecommend { get; set; }
    [JsonPropertyName("order_num")] public int OrderNum { get; set; }
    [JsonPropertyName("entry_no")] public int EntryNo { get; set; }
    [JsonPropertyName("deck_format")] public int? DeckFormat { get; set; }
}
