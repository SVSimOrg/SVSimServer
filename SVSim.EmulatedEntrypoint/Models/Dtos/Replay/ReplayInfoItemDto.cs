using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Replay;

/// <summary>
/// One entry in /replay/info's replay_list. Every numeric field ships as STRING
/// on the wire — matches prod capture verbatim (see data_dumps/captures/
/// traffic_prod_misc_clicking.ndjson, frame 96). The api-spec doc shows some
/// fields as number; capture overrides.
/// </summary>
[MessagePackObject]
public sealed class ReplayInfoItemDto
{
    [JsonPropertyName("battle_type"), Key("battle_type")]
    public string BattleType { get; set; } = "0";

    [JsonPropertyName("two_pick_type"), Key("two_pick_type")]
    public string TwoPickType { get; set; } = "0";

    [JsonPropertyName("deck_format"), Key("deck_format")]
    public string DeckFormat { get; set; } = "0";

    [JsonPropertyName("battle_id"), Key("battle_id")]
    public string BattleId { get; set; } = "0";

    [JsonPropertyName("is_limit_turn"), Key("is_limit_turn")]
    public string IsLimitTurn { get; set; } = "0";

    [JsonPropertyName("opponent_name"), Key("opponent_name")]
    public string OpponentName { get; set; } = "";

    [JsonPropertyName("class_id"), Key("class_id")]
    public string ClassId { get; set; } = "0";

    [JsonPropertyName("opponent_class_id"), Key("opponent_class_id")]
    public string OpponentClassId { get; set; } = "0";

    [JsonPropertyName("sub_class_id"), Key("sub_class_id")]
    public string SubClassId { get; set; } = "0";

    [JsonPropertyName("opponent_sub_class_id"), Key("opponent_sub_class_id")]
    public string OpponentSubClassId { get; set; } = "0";

    [JsonPropertyName("rotation_id"), Key("rotation_id")]
    public string RotationId { get; set; } = "0";

    [JsonPropertyName("opponent_rotation_id"), Key("opponent_rotation_id")]
    public string OpponentRotationId { get; set; } = "0";

    [JsonPropertyName("opponent_country_code"), Key("opponent_country_code")]
    public string OpponentCountryCode { get; set; } = "";

    [JsonPropertyName("chara_id"), Key("chara_id")]
    public string CharaId { get; set; } = "0";

    [JsonPropertyName("opponent_chara_id"), Key("opponent_chara_id")]
    public string OpponentCharaId { get; set; } = "0";

    [JsonPropertyName("opponent_emblem_id"), Key("opponent_emblem_id")]
    public string OpponentEmblemId { get; set; } = "0";

    [JsonPropertyName("opponent_degree_id"), Key("opponent_degree_id")]
    public string OpponentDegreeId { get; set; } = "0";

    [JsonPropertyName("is_win"), Key("is_win")]
    public string IsWin { get; set; } = "0";

    [JsonPropertyName("battle_start_time"), Key("battle_start_time")]
    public string BattleStartTime { get; set; } = "";

    [JsonPropertyName("create_time"), Key("create_time")]
    public string CreateTime { get; set; } = "";
}
