using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

[MessagePackObject]
public class SectionRequest : BaseRequest
{
    [JsonPropertyName("is_disp_first_tips")]
    [Key("is_disp_first_tips")]
    public bool IsDispFirstTips { get; set; }
}

[MessagePackObject]
public class SectionResponse
{
    [JsonPropertyName("world_list")]
    [Key("world_list")]
    public Dictionary<string, SectionWorld> WorldList { get; set; } = new();
}

[MessagePackObject]
public class SectionWorld
{
    [JsonPropertyName("title_text_id")]
    [Key("title_text_id")]
    public string TitleTextId { get; set; } = "";

    [JsonPropertyName("panel_image_name")]
    [Key("panel_image_name")]
    public string PanelImageName { get; set; } = "";

    [JsonPropertyName("ribbon_text")]
    [Key("ribbon_text")]
    public string RibbonText { get; set; } = "";

    [JsonPropertyName("is_complete")]
    [Key("is_complete")]
    public bool IsComplete { get; set; }

    [JsonPropertyName("section_list")]
    [Key("section_list")]
    public List<SectionEntry> SectionList { get; set; } = new();
}

[MessagePackObject]
public class SectionEntry
{
    [JsonPropertyName("section_id")]
    [Key("section_id")]
    public string SectionId { get; set; } = "";

    [JsonPropertyName("order_id")]
    [Key("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("all_story_order_id")]
    [Key("all_story_order_id")]
    public string AllStoryOrderId { get; set; } = "";

    [JsonPropertyName("name")]
    [Key("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("image_name")]
    [Key("image_name")]
    public string ImageName { get; set; } = "";

    [JsonPropertyName("is_leader_select")]
    [Key("is_leader_select")]
    public bool IsLeaderSelect { get; set; }

    [JsonPropertyName("back_ground_id")]
    [Key("back_ground_id")]
    public int BackGroundId { get; set; }

    [JsonPropertyName("is_finished")]
    [Key("is_finished")]
    public bool IsFinished { get; set; }

    [JsonPropertyName("released_chara_count")]
    [Key("released_chara_count")]
    public int ReleasedCharaCount { get; set; }

    [JsonPropertyName("finished_chara_count")]
    [Key("finished_chara_count")]
    public int FinishedCharaCount { get; set; }

    [JsonPropertyName("is_under_maintenance")]
    [Key("is_under_maintenance")]
    public bool IsUnderMaintenance { get; set; }

    [JsonPropertyName("chapter_select_type")]
    [Key("chapter_select_type")]
    public string ChapterSelectType { get; set; } = "1";

    [JsonPropertyName("story_type_overwrite")]
    [Key("story_type_overwrite")]
    public string StoryTypeOverwrite { get; set; } = "1";

    [JsonPropertyName("is_new")]
    [Key("is_new")]
    public bool IsNew { get; set; }

    [JsonPropertyName("is_play_another_end_appearance_animation")]
    [Key("is_play_another_end_appearance_animation")]
    public bool IsPlayAnotherEndAppearanceAnimation { get; set; }

    // Prod sends is_spoiler as 0/1 int (not bool) and spoiler_message as a SystemText key
    // (e.g. "story_section_14"). Used by limited-story sections that sit inside main-story
    // worlds — the client hides their title until you've cleared the gating main section.
    [JsonPropertyName("is_spoiler")]
    [Key("is_spoiler")]
    public int IsSpoiler { get; set; }

    [JsonPropertyName("spoiler_message")]
    [Key("spoiler_message")]
    public string SpoilerMessage { get; set; } = "";
}
