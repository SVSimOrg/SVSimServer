using MessagePack;
using SVSim.Database.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

[MessagePackObject]
public class IndexResponse
{
    #region Primitive Returns

    [JsonPropertyName("spot_point")]
    [Key("spot_point")]
    public int SpotPoint { get; set; }
    [JsonPropertyName("is_available_colosseum_free_entry")]
    [Key("is_available_colosseum_free_entry")]
    public bool IsAvailableColosseumFreeEntry { get; set; }
    [JsonPropertyName("friend_battle_invite_count")]
    [Key("friend_battle_invite_count")]
    public int FriendBattleInviteCount { get; set; }
    [JsonPropertyName("battle_recovery_status")]
    [Key("battle_recovery_status")]
    public int BattleRecoveryStatus { get; set; }
    [JsonPropertyName("room_recovery_status")]
    [Key("room_recovery_status")]
    public int RoomRecoveryStatus { get; set; }
    /// <summary>
    /// Prod emits this as bool (per the 2026-05-23 capture); the spec leaves it as a TODO
    /// (load-index.md line 296-297). We send bool to match prod; client's `.ToBoolean()`
    /// path handles either shape, but matching prod avoids the int-vs-bool drift noted in
    /// the seed-data-strategy crash audit.
    /// </summary>
    [JsonPropertyName("is_battle_pass_period")]
    [Key("is_battle_pass_period")]
    public bool IsBattlePassPeriod { get; set; }
    [JsonPropertyName("card_set_id_for_resource_dl_view")]
    [Key("card_set_id_for_resource_dl_view")]
    public int CardSetIdForResourceDlView { get; set; }
    // Serialized as wire deck_format via FormatJsonConverter (registered in Program.cs).
    [JsonPropertyName("deck_format")]
    [Key("deck_format")]
    public Format DeckFormat { get; set; } = Format.Rotation;

    /// <summary>
    /// Freshness trigger for the card-master refresh flow (Wizard/CardMaster.cs:18-30).
    /// Nullable + global <c>WhenWritingNull</c> means absence on the wire when the request
    /// already matches <c>CardMasterConfig.CurrentHash</c>. Presence (any value) tells the
    /// client to call <c>POST /immutable_data/card_master</c> with this echoed back; the
    /// client treats the string as opaque.
    /// <para>
    /// Lives on the inner <c>data</c> payload, NOT <c>data_headers</c> — verified by
    /// <c>LoadDetail.cs:414</c> constructing <c>new CardMaster.UpdateInfo(jsonData)</c>
    /// from the inner data, and the 2026-06-03 capture at
    /// <c>data_dumps/captures/traffic_prod_allstars_freepack.ndjson</c>.
    /// </para>
    /// </summary>
    [JsonPropertyName("card_master_hash")]
    [Key("card_master_hash")]
    public string? CardMasterHash { get; set; }

    #endregion

    #region Basic User Data

    [JsonPropertyName("user_tutorial")]
    [Key("user_tutorial")]
    public UserTutorial UserTutorial { get; set; } = new UserTutorial();

    [JsonPropertyName("user_info")]
    [Key("user_info")]
    public UserInfo UserInfo { get; set; } = new UserInfo();

    [JsonPropertyName("user_crystal_count")]
    [Key("user_crystal_count")]
    public UserCurrency UserCurrency { get; set; } = new UserCurrency();

    #endregion

    #region Inventory Data

    [JsonPropertyName("user_item_list")]
    [Key("user_item_list")]
    public List<UserItem> UserItems { get; set; } = new();

    [JsonPropertyName("user_deck_rotation")]
    [Key("user_deck_rotation")]
    public UserFormatDeckInfo UserRotationDecks { get; set; } = new();

    [JsonPropertyName("user_deck_unlimited")]
    [Key("user_deck_unlimited")]
    public UserFormatDeckInfo UserUnlimitedDecks { get; set; } = new();

    [JsonPropertyName("user_deck_my_rotation")]
    [Key("user_deck_my_rotation")]
    public UserFormatDeckInfo UserMyRotationDecks { get; set; } = new();

    [JsonPropertyName("user_card_list")]
    [Key("user_card_list")]
    public List<UserCard> UserCards { get; set; } = new();

    [JsonPropertyName("user_class_list")]
    [Key("user_class_list")]
    public List<UserClass> UserClasses { get; set; } = new();

    /// <summary>
    /// Wire is an array; parser iterates by index (LoadDetail.cs:358-360).
    /// </summary>
    [JsonPropertyName("user_sleeve_list")]
    [Key("user_sleeve_list")]
    public List<SleeveIdentifier> Sleeves { get; set; } = new();

    [JsonPropertyName("user_emblem_list")]
    [Key("user_emblem_list")]
    public List<EmblemIdentifier> UserEmblems { get; set; } = new();

    [JsonPropertyName("user_degree_list")]
    [Key("user_degree_list")]
    public List<DegreeIdentifier> UserDegrees { get; set; } = new();

    /// <summary>
    /// Wire is an array; parser iterates by index (LoadDetail.cs:348-356).
    /// </summary>
    [JsonPropertyName("user_leader_skin_list")]
    [Key("user_leader_skin_list")]
    public List<UserLeaderSkin> LeaderSkins { get; set; } = new();

    /// <summary>
    /// Wire is string[]; parser calls .ToString() on each element (LoadDetail.cs:387-392).
    /// </summary>
    [JsonPropertyName("user_mypage_list")]
    [Key("user_mypage_list")]
    public List<string> MyPageBackgrounds { get; set; } = new();

    #endregion

    #region Advanced Player Data

    /// <summary>
    /// Wire is an array of 5 entries; parser uses deck_format as discriminator
    /// (LoadDetail.cs:527-538).
    /// </summary>
    [JsonPropertyName("user_rank")]
    [Key("user_rank")]
    public List<UserRankInfo> UserRankInfo { get; set; } = new();

    [JsonPropertyName("user_rank_match_list")]
    [Key("user_rank_match_list")]
    public List<UserRankedMatches> UserRankedMatches { get; set; } = new();

    /// <summary>
    /// Spec: optional. Shape {normal?, total?, campaign?[]} — client parses three keys
    /// in LoadDetail.cs:553. <c>normal</c> presence triggers the login-bonus popup
    /// (NextSceneSwitcher.cs:103). Populated by ILoginBonusService at /load/index time;
    /// null when the viewer has already claimed today's bonus.
    /// </summary>
    [JsonPropertyName("daily_login_bonus")]
    [Key("daily_login_bonus")]
    public DailyLoginBonus? DailyLoginBonus { get; set; }

    [JsonPropertyName("challenge_config")]
    [Key("challenge_config")]
    public ArenaConfig ArenaConfig { get; set; } = new();

    #endregion

    #region Global Data

    [JsonPropertyName("red_ether_overwrite_list")]
    [Key("red_ether_overwrite_list")]
    public List<RedEtherOverride> RedEtherOverrides { get; set; } = new();

    /// <summary>
    /// Wire is a flat number[]; parser passes it straight to SetMaintenanceCardIds
    /// (LoadDetail.cs:165).
    /// </summary>
    [JsonPropertyName("maintenance_card_list")]
    [Key("maintenance_card_list")]
    public List<long> MaintenanceCards { get; set; } = new();

    /// <summary>
    /// Client's ArenaData(JsonData) ctor reads data[0] unconditionally inside the
    /// LoadDetail.cs:261 Keys.Contains("arena_info") branch — an empty list crashes
    /// /load/index with ArgumentOutOfRangeException. Send null (omitted on wire) when
    /// there's no arena to advertise.
    /// </summary>
    [JsonPropertyName("arena_info")]
    [Key("arena_info")]
    public List<ArenaInfo>? ArenaInfos { get; set; }

    /// <summary>
    /// Wire is an array; client uses POSITIONAL logic (index >= 24 = master ranks,
    /// LoadDetail.cs:417-422). Order must match repository's ordering.
    /// </summary>
    [JsonPropertyName("rank_info")]
    [Key("rank_info")]
    public List<RankInfo> RankInfo { get; set; } = new();

    /// <summary>
    /// Wire is an array; parser iterates by index (LoadDetail.cs:425-434).
    /// </summary>
    [JsonPropertyName("class_exp")]
    [Key("class_exp")]
    public List<ClassExp> ClassExp { get; set; } = new();

    [JsonPropertyName("loading_exclusion_card_list")]
    [Key("loading_exclusion_card_list")]
    public List<long> LoadingTipCardExclusions { get; set; } = new();

    [JsonPropertyName("default_setting")]
    [Key("default_setting")]
    public DefaultSettings DefaultSettings { get; set; } = new();

    [JsonPropertyName("unlimited_restricted_base_card_id_list")]
    [Key("unlimited_restricted_base_card_id_list")]
    public Dictionary<string, int> UnlimitedBanList { get; set; } = new();

    /// <summary>
    /// Client unconditionally accesses [1] and [Count-1] (LoadDetail.cs:184) 窶・list MUST
    /// have at least 2 entries or the client crashes.
    /// </summary>
    [JsonPropertyName("rotation_card_set_id_list")]
    [Key("rotation_card_set_id_list")]
    public List<CardSetIdentifier> RotationSets { get; set; } = new();

    /// <summary>
    /// Wire is a flat number[]; parser iterates and reads .ToInt() (LoadDetail.cs:463-468).
    /// </summary>
    [JsonPropertyName("reprinted_base_card_ids")]
    [Key("reprinted_base_card_ids")]
    public List<long> ReprintedCards { get; set; } = new();

    [JsonPropertyName("spot_cards")]
    [Key("spot_cards")]
    public Dictionary<string, int> SpotCards { get; set; } = new();

    [JsonPropertyName("pre_release_info")]
    [Key("pre_release_info")]
    public PreReleaseInfo? PreReleaseInfo { get; set; }

    [JsonPropertyName("my_rotation_info")]
    [Key("my_rotation_info")]
    public MyRotationInfo? MyRotationInfo { get; set; }

    [JsonPropertyName("avatar_info")]
    [Key("avatar_info")]
    public AvatarInfo? AvatarRotationInfo { get; set; }

    [JsonPropertyName("feature_maintenance_list")]
    [Key("feature_maintenance_list")]
    public List<FeatureMaintenance> FeatureMaintenances { get; set; } = new();

    [JsonPropertyName("special_crystal_info")]
    [Key("special_crystal_info")]
    public List<SpecialCrystalInfo> SpecialCrystalInfos { get; set; } = new();

    /// <summary>
    /// Spec: optional, Record&lt;string, BattlePassLevelInfo&gt; keyed by level-as-string
    /// (load-index.md:228). Omit (null) when no Battle Pass is active.
    /// </summary>
    [JsonPropertyName("battle_pass_level_info")]
    [Key("battle_pass_level_info")]
    public IReadOnlyDictionary<string, BattlePassLevel>? BattlePassLevelInfo { get; set; }

    /// <summary>
    /// Wire is string[]; parser calls .ToString() on each element (LoadDetail.cs:493-499).
    /// </summary>
    [JsonPropertyName("open_battle_field_id_list")]
    [Key("open_battle_field_id_list")]
    public List<string> OpenBattlefieldIds { get; set; } = new();

    #endregion

    #region Misc Data

    [JsonPropertyName("loot_box_regulation")]
    [Key("loot_box_regulation")]
    public LootBoxRegulations LootBoxRegulations { get; set; } = new();

    [JsonPropertyName("gathering_info")]
    [Key("gathering_info")]
    public GatheringInfo GatheringInfo { get; set; } = new();

    /// <summary>
    /// Spec is unclear whether this is returned at /load/index or only at /config/* endpoints
    /// (load-index.md line 390). Pending live-capture confirmation; harmless extra.
    /// </summary>
    [JsonPropertyName("user_config")]
    [Key("user_config")]
    public UserConfig UserConfig { get; set; } = new();

    #endregion
}
