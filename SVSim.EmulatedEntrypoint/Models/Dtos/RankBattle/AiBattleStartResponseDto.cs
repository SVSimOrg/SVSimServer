using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.RankBattle;

[MessagePackObject(keyAsPropertyName: true)]
public sealed class AiBattleStartResponseDto
{
    [JsonPropertyName("ai_id")]
    [Key("ai_id")]
    public int AiId { get; set; }

    [JsonPropertyName("turnState")]
    [Key("turnState")]
    public int TurnState { get; set; }

    [JsonPropertyName("self_info")]
    [Key("self_info")]
    public AiBattlePlayerInfo SelfInfo { get; set; } = new();

    [JsonPropertyName("oppo_info")]
    [Key("oppo_info")]
    public AiBattlePlayerInfo OppoInfo { get; set; } = new();
}

/// <summary>
/// Per docs/api-spec/endpoints/post-login/rank-battle/ai-start.md — the AI battle
/// subsystem uses camelCase keys (sleeveId, emblemId, ...), not the project-default
/// snake_case. The [JsonPropertyName] overrides bypass the global SnakeCaseLower
/// policy. country_code / self_info / oppo_info are the two outliers staying snake_case.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public sealed class AiBattlePlayerInfo
{
    [JsonPropertyName("country_code")]
    [Key("country_code")]
    public string CountryCode { get; set; } = "NONE";

    [JsonPropertyName("userName")]
    [Key("userName")]
    public string UserName { get; set; } = "NONE";

    [JsonPropertyName("sleeveId")]
    [Key("sleeveId")]
    public int SleeveId { get; set; } = -1;

    [JsonPropertyName("emblemId")]
    [Key("emblemId")]
    public int EmblemId { get; set; } = -1;

    [JsonPropertyName("degreeId")]
    [Key("degreeId")]
    public int DegreeId { get; set; } = -1;

    [JsonPropertyName("fieldId")]
    [Key("fieldId")]
    public int FieldId { get; set; } = -1;

    [JsonPropertyName("isOfficial")]
    [Key("isOfficial")]
    public int IsOfficial { get; set; } = -1;

    [JsonPropertyName("oppoId")]
    [Key("oppoId")]
    public int OppoId { get; set; } = -1;

    [JsonPropertyName("seed")]
    [Key("seed")]
    public int Seed { get; set; } = -1;

    [JsonPropertyName("rank")]
    [Key("rank")]
    public int Rank { get; set; } = -1;

    [JsonPropertyName("battlePoint")]
    [Key("battlePoint")]
    public int BattlePoint { get; set; } = -1;

    [JsonPropertyName("classId")]
    [Key("classId")]
    public int ClassId { get; set; } = -1;

    [JsonPropertyName("charaId")]
    [Key("charaId")]
    public int CharaId { get; set; } = -1;

    [JsonPropertyName("isMasterRank")]
    [Key("isMasterRank")]
    public int IsMasterRank { get; set; } = -1;

    [JsonPropertyName("masterPoint")]
    [Key("masterPoint")]
    public int MasterPoint { get; set; } = -1;
}
