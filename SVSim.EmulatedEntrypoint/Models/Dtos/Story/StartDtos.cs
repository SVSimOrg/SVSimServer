using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

[MessagePackObject]
public class StartRequest : BaseRequest
{
    [JsonPropertyName("story_ids")]
    [Key("story_ids")]
    public int[] StoryIds { get; set; } = Array.Empty<int>();
}

// The `start` response is dynamic — each numeric key corresponds to a request story_ids index.
// We use a Dictionary<string, object> to support both the populated and empty slot shapes.
// MessagePack handles Dictionary natively; no [MessagePackObject] needed here.
public class StartResponse : Dictionary<string, object>
{
    public void AddSlot(int index, object slotPayload) => this[index.ToString()] = slotPayload;
}

[MessagePackObject]
public class StartSlotWithSbs
{
    [JsonPropertyName("special_battle_setting")]
    [Key("special_battle_setting")]
    public SpecialBattleSettingDto SpecialBattleSetting { get; set; } = new();
}

[MessagePackObject]
public class SpecialBattleSettingDto
{
    [JsonPropertyName("id")]
    [Key("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("player_first_turn")]
    [Key("player_first_turn")]
    public string PlayerFirstTurn { get; set; } = "";

    [JsonPropertyName("player_start_pp")]
    [Key("player_start_pp")]
    public string PlayerStartPp { get; set; } = "";

    [JsonPropertyName("enemy_start_pp")]
    [Key("enemy_start_pp")]
    public string EnemyStartPp { get; set; } = "";

    [JsonPropertyName("player_start_life")]
    [Key("player_start_life")]
    public string PlayerStartLife { get; set; } = "";

    [JsonPropertyName("enemy_start_life")]
    [Key("enemy_start_life")]
    public string EnemyStartLife { get; set; } = "";

    [JsonPropertyName("player_attach_skill")]
    [Key("player_attach_skill")]
    public string PlayerAttachSkill { get; set; } = "";

    [JsonPropertyName("enemy_attach_skill")]
    [Key("enemy_attach_skill")]
    public string EnemyAttachSkill { get; set; } = "";

    [JsonPropertyName("id_override_in_battle_log")]
    [Key("id_override_in_battle_log")]
    public string IdOverrideInBattleLog { get; set; } = "";

    [JsonPropertyName("banish_effect_override")]
    [Key("banish_effect_override")]
    public string BanishEffectOverride { get; set; } = "";

    [JsonPropertyName("token_draw_effect_override")]
    [Key("token_draw_effect_override")]
    public string TokenDrawEffectOverride { get; set; } = "";

    [JsonPropertyName("special_token_draw_effect_override")]
    [Key("special_token_draw_effect_override")]
    public string SpecialTokenDrawEffectOverride { get; set; } = "";

    [JsonPropertyName("result_skip")]
    [Key("result_skip")]
    public string ResultSkip { get; set; } = "";

    [JsonPropertyName("vs_effect_override")]
    [Key("vs_effect_override")]
    public string VsEffectOverride { get; set; } = "";

    [JsonPropertyName("class_destroy_effect_override")]
    [Key("class_destroy_effect_override")]
    public string ClassDestroyEffectOverride { get; set; } = "";

    [JsonPropertyName("note")]
    [Key("note")]
    public string Note { get; set; } = "";
}
