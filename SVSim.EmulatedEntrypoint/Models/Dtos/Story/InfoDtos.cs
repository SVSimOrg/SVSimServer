using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

[MessagePackObject]
public class InfoRequest : BaseRequest
{
    [JsonPropertyName("section_id")]
    [Key("section_id")]
    public int SectionId { get; set; }

    [JsonPropertyName("chara_id")]
    [Key("chara_id")]
    public int CharaId { get; set; }   // 0 for non-leader-select
}

[MessagePackObject]
public class InfoResponse
{
    [JsonPropertyName("story_master_list")]
    [Key("story_master_list")]
    public List<StoryMasterEntry> StoryMasterList { get; set; } = new();

    [JsonPropertyName("maintenance_card_list")]
    [Key("maintenance_card_list")]
    public List<long> MaintenanceCardList { get; set; } = new();
}

[MessagePackObject]
public class StoryMasterEntry
{
    [JsonPropertyName("story_id")]
    [Key("story_id")]
    public string StoryId { get; set; } = "";

    [JsonPropertyName("section_id")]
    [Key("section_id")]
    public string SectionId { get; set; } = "";

    [JsonPropertyName("chara_id")]
    [Key("chara_id")]
    public string CharaId { get; set; } = "";

    [JsonPropertyName("chapter_id")]
    [Key("chapter_id")]
    public string ChapterId { get; set; } = "";

    [JsonPropertyName("is_lock")]
    [Key("is_lock")]
    public bool IsLock { get; set; }

    [JsonPropertyName("next_chapter_id")]
    [Key("next_chapter_id")]
    public string NextChapterId { get; set; } = "";

    [JsonPropertyName("required_chapter_id")]
    [Key("required_chapter_id")]
    public string RequiredChapterId { get; set; } = "";

    [JsonPropertyName("selection_display_position")]
    [Key("selection_display_position")]
    public string SelectionDisplayPosition { get; set; } = "";

    [JsonPropertyName("selection_text_id")]
    [Key("selection_text_id")]
    public string SelectionTextId { get; set; } = "";

    [JsonPropertyName("show_coordinate")]
    [Key("show_coordinate")]
    public string ShowCoordinate { get; set; } = "";

    [JsonPropertyName("x_coordinate")]
    [Key("x_coordinate")]
    public string XCoordinate { get; set; } = "";

    [JsonPropertyName("y_coordinate")]
    [Key("y_coordinate")]
    public string YCoordinate { get; set; } = "";

    // Wire typo preserved: note the space in "is_camera_ movable"
    [JsonPropertyName("is_camera_ movable")]
    [Key("is_camera_ movable")]
    public string IsCameraMovable { get; set; } = "";

    [JsonPropertyName("show_subtitles")]
    [Key("show_subtitles")]
    public string ShowSubtitles { get; set; } = "";

    [JsonPropertyName("battle_exists")]
    [Key("battle_exists")]
    public bool BattleExists { get; set; }

    [JsonPropertyName("enemy_chara_id")]
    [Key("enemy_chara_id")]
    public string EnemyCharaId { get; set; } = "";

    [JsonPropertyName("enemy_class")]
    [Key("enemy_class")]
    public string EnemyClass { get; set; } = "";

    [JsonPropertyName("enemy_ai_id")]
    [Key("enemy_ai_id")]
    public string EnemyAiId { get; set; } = "";

    [JsonPropertyName("bg_file_name")]
    [Key("bg_file_name")]
    public string BgFileName { get; set; } = "";

    [JsonPropertyName("chapter_effect_path")]
    [Key("chapter_effect_path")]
    public string ChapterEffectPath { get; set; } = "";

    [JsonPropertyName("chapter_clear_text_id")]
    [Key("chapter_clear_text_id")]
    public string ChapterClearTextId { get; set; } = "";

    [JsonPropertyName("battle3dfield_id")]
    [Key("battle3dfield_id")]
    public string Battle3dFieldId { get; set; } = "";

    [JsonPropertyName("bgm_id")]
    [Key("bgm_id")]
    public string BgmId { get; set; } = "";

    [JsonPropertyName("special_battle_setting_id")]
    [Key("special_battle_setting_id")]
    public string SpecialBattleSettingId { get; set; } = "";

    [JsonPropertyName("release_point")]
    [Key("release_point")]
    public string ReleasePoint { get; set; } = "";

    [JsonPropertyName("battle_settings")]
    [Key("battle_settings")]
    public List<BattleSettingDto> BattleSettings { get; set; } = new();

    [JsonPropertyName("story_reward")]
    [Key("story_reward")]
    public List<RewardDto> StoryReward { get; set; } = new();

    [JsonPropertyName("is_maintenance_chapter")]
    [Key("is_maintenance_chapter")]
    public bool IsMaintenanceChapter { get; set; }

    [JsonPropertyName("is_released")]
    [Key("is_released")]
    public bool IsReleased { get; set; }

    [JsonPropertyName("is_skipped")]
    [Key("is_skipped")]
    public bool IsSkipped { get; set; }

    [JsonPropertyName("is_finish")]
    [Key("is_finish")]
    public bool IsFinish { get; set; }

    [JsonPropertyName("unlock_text")]
    [Key("unlock_text")]
    public string UnlockText { get; set; } = "";

    [JsonPropertyName("is_play_another_end_appearance_animation")]
    [Key("is_play_another_end_appearance_animation")]
    public bool IsPlayAnotherEndAppearanceAnimation { get; set; }

    [JsonPropertyName("is_released_another_end")]
    [Key("is_released_another_end")]
    public bool IsReleasedAnotherEnd { get; set; }

    [JsonPropertyName("is_skip_enabled")]
    [Key("is_skip_enabled")]
    public bool IsSkipEnabled { get; set; }

    // Optional — prod omits the key entirely on chapters without sub-chapters. Only emitted for
    // chapters that split into N narrative vignettes (e.g. section 9 ch.13 has 5 sub-chapters).
    // The client uses each sub's is_finish flag to derive the parent's ChapterClearStatus
    // (AllCleared / AlreadyRead / NotCleared per StoryChapterData.GetClearStatusUsingSubChapter).
    // Explicit WhenWritingNull (rather than relying on global policy) so the key is dropped
    // under any serializer config — including the wire-shape snapshot test which sets
    // DefaultIgnoreCondition=Never to exercise every populated field.
    [JsonPropertyName("sub_chapters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Key("sub_chapters")]
    public List<SubChapterDto>? SubChapters { get; set; }
}

[MessagePackObject]
public class SubChapterDto
{
    [JsonPropertyName("story_id")]
    [Key("story_id")]
    public int StoryId { get; set; }

    [JsonPropertyName("sub_chapter_id")]
    [Key("sub_chapter_id")]
    public int SubChapterId { get; set; }

    [JsonPropertyName("is_finish")]
    [Key("is_finish")]
    public bool IsFinish { get; set; }

    [JsonPropertyName("is_maintenance_chapter")]
    [Key("is_maintenance_chapter")]
    public bool IsMaintenanceChapter { get; set; }
}

[MessagePackObject]
public class BattleSettingDto
{
    [JsonPropertyName("deck_class_id")]
    [Key("deck_class_id")]
    public int DeckClassId { get; set; }

    [JsonPropertyName("player_emotion_override")]
    [Key("player_emotion_override")]
    public int PlayerEmotionOverride { get; set; }

    [JsonPropertyName("enemy_emotion_override")]
    [Key("enemy_emotion_override")]
    public int EnemyEmotionOverride { get; set; }

    [JsonPropertyName("skin_id_override")]
    [Key("skin_id_override")]
    public int SkinIdOverride { get; set; }

    [JsonPropertyName("battle3dfield_id_override")]
    [Key("battle3dfield_id_override")]
    public int Battle3dFieldIdOverride { get; set; }

    [JsonPropertyName("bgm_id_override")]
    [Key("bgm_id_override")]
    public int BgmIdOverride { get; set; }

    [JsonPropertyName("deck_skin_id_override")]
    [Key("deck_skin_id_override")]
    public int DeckSkinIdOverride { get; set; }
}

[MessagePackObject]
public class RewardDto
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public string RewardType { get; set; } = "";

    [JsonPropertyName("reward_detail_id")]
    [Key("reward_detail_id")]
    public string RewardDetailId { get; set; } = "";

    [JsonPropertyName("reward_number")]
    [Key("reward_number")]
    public string RewardNumber { get; set; } = "";
}
