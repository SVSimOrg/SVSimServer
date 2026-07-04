using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class PracticeOpponentSeed
{
    [JsonPropertyName("practice_id")] public int PracticeId { get; set; }
    [JsonPropertyName("text_id")] public string TextId { get; set; } = "";
    [JsonPropertyName("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("chara_id")] public int CharaId { get; set; }
    [JsonPropertyName("degree_id")] public int DegreeId { get; set; }
    [JsonPropertyName("ai_deck_level")] public int AiDeckLevel { get; set; }
    [JsonPropertyName("ai_logic_level")] public int AiLogicLevel { get; set; }
    [JsonPropertyName("ai_max_life")] public int AiMaxLife { get; set; }
    [JsonPropertyName("battle3dfield_id")] public string Battle3dFieldId { get; set; } = "1";
    [JsonPropertyName("is_maintenance")] public bool IsMaintenance { get; set; }
    [JsonPropertyName("is_campaign_practice")] public bool IsCampaignPractice { get; set; }
}
