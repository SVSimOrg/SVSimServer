using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class BotRosterSeed
{
    [JsonPropertyName("ai_id")] public int AiId { get; set; }
    [JsonPropertyName("country_code")] public string CountryCode { get; set; } = "";
    [JsonPropertyName("user_name")] public string UserName { get; set; } = "";
    [JsonPropertyName("sleeve_id")] public int SleeveId { get; set; }
    [JsonPropertyName("emblem_id")] public int EmblemId { get; set; }
    [JsonPropertyName("degree_id")] public int DegreeId { get; set; }
    [JsonPropertyName("field_id")] public int FieldId { get; set; }
    [JsonPropertyName("is_official")] public int IsOfficial { get; set; }
    [JsonPropertyName("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("chara_id")] public int CharaId { get; set; }
    [JsonPropertyName("rank")] public int Rank { get; set; }
    [JsonPropertyName("battle_point")] public int BattlePoint { get; set; }
    [JsonPropertyName("is_master_rank")] public int IsMasterRank { get; set; }
    [JsonPropertyName("master_point")] public int MasterPoint { get; set; }
}
