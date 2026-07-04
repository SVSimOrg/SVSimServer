using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Story;

// GatheringInfo and CompetitionInfo resolve via the parent Models.Dtos namespace (C# walks outward).

[MessagePackObject]
public class FinishRequest : BaseRequest
{
    [JsonPropertyName("story_id")]
    [Key("story_id")]
    public int StoryId { get; set; }

    [JsonPropertyName("is_finish")]
    [Key("is_finish")]
    public int IsFinish { get; set; }

    // Battle-shape fields (present only on play-shape)
    [JsonPropertyName("evolve_count")]
    [Key("evolve_count")]
    public int? EvolveCount { get; set; }

    [JsonPropertyName("total_turn")]
    [Key("total_turn")]
    public int? TotalTurn { get; set; }

    [JsonPropertyName("deck_no")]
    [Key("deck_no")]
    public int? DeckNo { get; set; }

    [JsonPropertyName("use_build_deck")]
    [Key("use_build_deck")]
    public int? UseBuildDeck { get; set; }

    [JsonPropertyName("deck_format")]
    [Key("deck_format")]
    public int? DeckFormat { get; set; }

    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int? ClassId { get; set; }

    [JsonPropertyName("mission")]
    [Key("mission")]
    public Dictionary<string, int>? Mission { get; set; }

    [JsonPropertyName("recovery_data")]
    [Key("recovery_data")]
    public string? RecoveryData { get; set; }

    // Misspelled the same way in every solo finish endpoint — preserved on the wire.
    [JsonPropertyName("prosessing_time_data")]
    [Key("prosessing_time_data")]
    public string[]? ProsessingTimeData { get; set; }

    // No-battle-shape fields
    [JsonPropertyName("selection_chapter_id")]
    [Key("selection_chapter_id")]
    public string? SelectionChapterId { get; set; }

    [JsonPropertyName("is_select_another_end")]
    [Key("is_select_another_end")]
    public bool? IsSelectAnotherEnd { get; set; }

    /// <summary>
    /// Derived: true when the request carries battle-shape fields (ClassId present = play-shape).
    /// Kept off both serializations.
    /// </summary>
    [JsonIgnore]
    [IgnoreMember]
    public bool IsPlayShape => ClassId.HasValue;
}

[MessagePackObject]
public class FinishResponse
{
    [JsonPropertyName("get_class_experience")]
    [Key("get_class_experience")]
    public string GetClassExperience { get; set; } = "0";

    [JsonPropertyName("class_experience")]
    [Key("class_experience")]
    public int ClassExperience { get; set; }

    [JsonPropertyName("class_level")]
    [Key("class_level")]
    public string ClassLevel { get; set; } = "0";

    [JsonPropertyName("achieved_info")]
    [Key("achieved_info")]
    public Dictionary<string, object> AchievedInfo { get; set; } = new();

    [JsonPropertyName("reward_list")]
    [Key("reward_list")]
    public List<RewardGrant> RewardList { get; set; } = new();

    [JsonPropertyName("story_reward_list")]
    [Key("story_reward_list")]
    public List<RewardGrant> StoryRewardList { get; set; } = new();

    // ─── Post-action mypage badge cluster ───
    //
    // MyPageNotifications.ParseBadgeInfos (Wizard/MyPageNotifications.cs:9) reads every key below
    // unguardedly; omitting any one throws KeyNotFoundException in Cute.NetworkManager.Connect and
    // aborts the response. The same cluster ships from every endpoint that calls ParseBadgeInfos
    // (StoryFinishTask, QuestFinishTask, RecoveryTask, OpenRoomBattleGetRecoveryParamTask).

    [JsonPropertyName("quest")]
    [Key("quest")]
    public BadgeFlag Quest { get; set; } = new();

    [JsonPropertyName("story_notification")]
    [Key("story_notification")]
    public BadgeFlag StoryNotification { get; set; } = new();

    [JsonPropertyName("basic_puzzle")]
    [Key("basic_puzzle")]
    public BadgeFlag BasicPuzzle { get; set; } = new();

    [JsonPropertyName("shop_notification")]
    [Key("shop_notification")]
    public ShopNotificationBadges ShopNotification { get; set; } = new();

    [JsonPropertyName("receive_friend_apply_count")]
    [Key("receive_friend_apply_count")]
    public int ReceiveFriendApplyCount { get; set; }

    [JsonPropertyName("gathering_info")]
    [Key("gathering_info")]
    public GatheringInfo GatheringInfo { get; set; } = new();

    [JsonPropertyName("competition_info")]
    [Key("competition_info")]
    public CompetitionInfo CompetitionInfo { get; set; } = new();

    [JsonPropertyName("is_available_colosseum_free_entry")]
    [Key("is_available_colosseum_free_entry")]
    public bool IsAvailableColosseumFreeEntry { get; set; }
}

[MessagePackObject]
public class RewardGrant
{
    [JsonPropertyName("reward_type")]
    [Key("reward_type")]
    public string RewardType { get; set; } = "";

    [JsonPropertyName("reward_id")]
    [Key("reward_id")]
    public string RewardId { get; set; } = "";

    [JsonPropertyName("reward_num")]
    [Key("reward_num")]
    public string RewardNum { get; set; } = "";
}
