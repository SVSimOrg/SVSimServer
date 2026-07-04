using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Admin;

/// <summary>
/// Snake-case JSON. Only used by the import endpoint (plain JSON over HTTP, not the
/// Unity msgpack path) so no MessagePack attributes are needed.
/// </summary>
public class ImportViewerRequest
{
    [JsonPropertyName("steam_id")] public ulong SteamId { get; set; }

    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
    [JsonPropertyName("country_code")] public string? CountryCode { get; set; }
    [JsonPropertyName("tutorial_state")] public int? TutorialState { get; set; }

    [JsonPropertyName("selected_emblem_id")] public int? SelectedEmblemId { get; set; }
    [JsonPropertyName("selected_degree_id")] public int? SelectedDegreeId { get; set; }

    [JsonPropertyName("currency")] public ImportCurrency? Currency { get; set; }

    [JsonPropertyName("owned_sleeve_ids")] public List<int>? OwnedSleeveIds { get; set; }
    [JsonPropertyName("owned_emblem_ids")] public List<int>? OwnedEmblemIds { get; set; }
    [JsonPropertyName("owned_degree_ids")] public List<int>? OwnedDegreeIds { get; set; }
    [JsonPropertyName("owned_leader_skin_ids")] public List<int>? OwnedLeaderSkinIds { get; set; }
    [JsonPropertyName("owned_mypage_background_ids")] public List<int>? OwnedMyPageBackgroundIds { get; set; }

    [JsonPropertyName("classes")] public List<ImportClassData>? Classes { get; set; }

    [JsonPropertyName("owned_cards")] public List<ImportCard>? OwnedCards { get; set; }

    [JsonPropertyName("items")] public List<ImportItem>? Items { get; set; }

    [JsonPropertyName("decks")] public List<ImportDeck>? Decks { get; set; }

    [JsonPropertyName("mission_meta")] public ImportMissionMeta? MissionMeta { get; set; }

    [JsonPropertyName("missions")] public List<ImportMission>? Missions { get; set; }

    [JsonPropertyName("achievements")]
    public List<ImportAchievement>? Achievements { get; set; }

    [JsonPropertyName("story_progress")]
    public List<ImportStoryProgress>? StoryProgress { get; set; }
}

public class ImportDeck
{
    [JsonPropertyName("deck_format")]           public int DeckFormat { get; set; } // wire code; map via FormatExtensions.FromApi
    [JsonPropertyName("deck_no")]               public int DeckNo { get; set; }
    [JsonPropertyName("deck_name")]             public string? DeckName { get; set; }
    [JsonPropertyName("class_id")]              public int ClassId { get; set; }
    [JsonPropertyName("card_id_array")]         public List<long>? CardIdArray { get; set; }
    [JsonPropertyName("sleeve_id")]             public long? SleeveId { get; set; }
    [JsonPropertyName("leader_skin_id")]        public int? LeaderSkinId { get; set; }
    [JsonPropertyName("is_random_leader_skin")] public int? IsRandomLeaderSkin { get; set; }
    // Prod emits rotation_id as a numeric string ("10008") for real MyRotation decks but a bare
    // number (0) for unset slots; FlexibleStringConverter accepts either (a plain string? 400s on
    // the numeric form because AllowReadingFromString only covers string→number).
    [JsonPropertyName("my_rotation_id")] [JsonConverter(typeof(FlexibleStringConverter))]
    public string? MyRotationId { get; set; }
}

public class ImportCurrency
{
    [JsonPropertyName("crystals")] public ulong? Crystals { get; set; }
    [JsonPropertyName("rupees")] public ulong? Rupees { get; set; }
    [JsonPropertyName("red_ether")] public ulong? RedEther { get; set; }
}

public class ImportClassData
{
    [JsonPropertyName("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("exp")] public int Exp { get; set; }
}

public class ImportCard
{
    [JsonPropertyName("card_id")]      public long CardId { get; set; }
    [JsonPropertyName("count")]        public int Count { get; set; }
    [JsonPropertyName("is_protected")] public bool IsProtected { get; set; }
}

public class ImportItem
{
    [JsonPropertyName("item_id")] public int ItemId { get; set; }
    [JsonPropertyName("count")]   public int Count { get; set; }
}

public class ImportMission
{
    [JsonPropertyName("mission_id")]
    public int MissionId { get; set; }

    [JsonPropertyName("mission_status")]
    public int MissionStatus { get; set; }

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("slot")]
    public int? Slot { get; set; }
}

public class ImportAchievement
{
    [JsonPropertyName("achievement_type")]          public int AchievementType { get; set; }
    [JsonPropertyName("level")]                     public int Level { get; set; }
    [JsonPropertyName("now_achieved_level")]        public int NowAchievedLevel { get; set; }
    [JsonPropertyName("result_announce_saw_level")] public int ResultAnnounceSawLevel { get; set; }
    [JsonPropertyName("total_count")]               public int TotalCount { get; set; }
}

public class ImportStoryProgress
{
    [JsonPropertyName("story_api_type")] public int  StoryApiType { get; set; }
    [JsonPropertyName("story_id")]       public int  StoryId { get; set; }
    [JsonPropertyName("sub_chapter_id")] public int? SubChapterId { get; set; }
    [JsonPropertyName("is_finish")]      public bool IsFinish { get; set; }
    [JsonPropertyName("is_skipped")]     public bool IsSkipped { get; set; }
}

public class ImportMissionMeta
{
    // Wire key is `is_received_two_pick_mission` (loader-captured verbatim from /load/index
    // user_info); the field ships as a string "1"/"0" not a bool. Keep the DTO type as int?
    // so binding matches; the controller normalizes to bool.
    [JsonPropertyName("is_received_two_pick_mission")]
    public int? HasReceivedPickTwoMission { get; set; }

    [JsonPropertyName("mission_receive_type")]
    public int? MissionReceiveType { get; set; }

    // Wire ships this as a "yyyy-MM-dd HH:mm:ss" string, NOT a Unix long (see
    // FriendService.DefaultMissionChangeTime for the canonical game format). Controller
    // parses via DateTime.TryParseExact with invariant culture.
    [JsonPropertyName("mission_change_time")]
    public string? MissionChangeTime { get; set; }
}
