using MessagePack;
using SVSim.Database.Enums;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

/// <summary>
/// /mypage/index ("home screen refresh") response payload.
///
/// Required fields per the minimum-viable section of
/// docs/api-spec/endpoints/post-login/mypage-index.md and corroborated by
/// MyPageTask.cs direct-index accesses (jsonData["…"] without TryGetValue).
/// Optional fields are nullable and omitted by the global WhenWritingNull
/// policy — the client uses TryGetValue / GetValueOrDefault for those.
/// </summary>
[MessagePackObject]
public class MyPageIndexResponse
{
    // ── User identity / counts ─────────────────────────────────────────────

    /// <summary>
    /// Full UserInfo block. Client only reads .name here (MyPageTask.cs:39) but
    /// prod emits the full structure, so we do too.
    /// </summary>
    [JsonPropertyName("user_info")]
    [Key("user_info")]
    public UserInfo UserInfo { get; set; } = new();

    [JsonPropertyName("unreceived_mission_reward_count")]
    [Key("unreceived_mission_reward_count")]
    public int UnreceivedMissionRewardCount { get; set; }

    [JsonPropertyName("receive_friend_apply_count")]
    [Key("receive_friend_apply_count")]
    public int ReceiveFriendApplyCount { get; set; }

    [JsonPropertyName("unread_present_count")]
    [Key("unread_present_count")]
    public int UnreadPresentCount { get; set; }

    [JsonPropertyName("friend_battle_invite_count")]
    [Key("friend_battle_invite_count")]
    public int FriendBattleInviteCount { get; set; }

    // ── Guild ──────────────────────────────────────────────────────────────

    [JsonPropertyName("guild_notification")]
    [Key("guild_notification")]
    public GuildNotification GuildNotification { get; set; } = new();

    // ── Announcements ──────────────────────────────────────────────────────

    [JsonPropertyName("last_announce_id")]
    [Key("last_announce_id")]
    public int LastAnnounceId { get; set; }

    /// <summary>ISO datetime. Parse is wrapped in try/catch on the client.</summary>
    [JsonPropertyName("last_announce_update_time")]
    [Key("last_announce_update_time")]
    public string LastAnnounceUpdateTime { get; set; } = string.Empty;

    // ── Maintenance ────────────────────────────────────────────────────────

    /// <summary>Same shape as /load/index. Empty list in the 2026-05-23 capture.</summary>
    [JsonPropertyName("feature_maintenance_list")]
    [Key("feature_maintenance_list")]
    public List<FeatureMaintenance> FeatureMaintenanceList { get; set; } = new();

    // ── Arena / Colosseum ──────────────────────────────────────────────────

    /// <summary>
    /// Client unconditionally constructs ArenaData(arena_info) which reads [0],
    /// so this MUST be a non-empty list. See LoadController BuildArenaInfosAsync
    /// — we mirror that, returning null (omitted on wire) when no Take Two
    /// season is seeded, in which case the client's Keys.Contains guard at
    /// LoadDetail.cs:261 handles it. For mypage there is no equivalent guard;
    /// the client always reads it. Until that's reconciled we send a minimal
    /// stub on the controller side.
    /// </summary>
    [JsonPropertyName("arena_info")]
    [Key("arena_info")]
    public List<ArenaInfo> ArenaInfo { get; set; } = new();

    [JsonPropertyName("is_arena_challenge_period")]
    [Key("is_arena_challenge_period")]
    public bool IsArenaChallengePeriod { get; set; }

    [JsonPropertyName("is_available_colosseum_free_entry")]
    [Key("is_available_colosseum_free_entry")]
    public bool IsAvailableColosseumFreeEntry { get; set; }

    // ── Sealed Arena season ────────────────────────────────────────────────

    /// <summary>
    /// sealed_info is consumed by ArenaData.SetSealedMyPageResponseData (Keys.Contains-guarded),
    /// but post-parse-consumer policy says we emit anyway. Defaults to a zeroed-out SealedInfo
    /// when no current season is seeded — Enable=0 means the UI treats Sealed as inactive.
    /// </summary>
    [JsonPropertyName("sealed_info")]
    [Key("sealed_info")]
    public SealedInfo SealedInfo { get; set; } = new();

    // ── Mypage banner carousel ─────────────────────────────────────────────

    /// <summary>
    /// banner is consumed by per-entry parsing inside a TryGetValue guard
    /// (Wizard/MyPageBannerBase.BannerInfo.Parse iterates the array if present). We always emit
    /// the list — empty when no rows have been imported. See SVSim.Bootstrap.MyPageGlobalsImporter.ImportBannersAsync.
    /// </summary>
    [JsonPropertyName("banner")]
    [Key("banner")]
    public List<BannerInfo> Banner { get; set; } = new();

    /// <summary>Prod sends explicit null. Override WhenWritingNull so the key survives serialization.</summary>
    [JsonPropertyName("sub_banner")]
    [Key("sub_banner")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object? SubBanner { get; set; }

    [JsonPropertyName("sub_banner_list")]
    [Key("sub_banner_list")]
    public List<object> SubBannerList { get; set; } = new();

    [JsonPropertyName("home_dialog_list")]
    [Key("home_dialog_list")]
    public List<Common.HomeDialog> HomeDialogList { get; set; } = new();

    // ── Room type in session (Special-format windows) ──────────────────────

    [JsonPropertyName("room_type_in_session")]
    [Key("room_type_in_session")]
    public RoomTypeInSession RoomTypeInSession { get; set; } = new();

    /// <summary>
    /// Required — ColosseumEntryInfoTask.SetColosseumInfo indexes this key
    /// directly (Wizard/ColosseumEntryInfoTask.cs:102) and reads
    /// is_colosseum_period without a guard. Built by
    /// <see cref="ColosseumLobbyInfoBuilder"/> from <c>ColosseumSeasonConfig</c> —
    /// shared with <c>/arena_colosseum/{top,get_fee_info}</c>.
    /// </summary>
    [JsonPropertyName("colosseum_info")]
    [Key("colosseum_info")]
    public ColosseumLobbyInfo ColosseumInfo { get; set; } = new();

    // ── Convention / offline event ─────────────────────────────────────────

    [JsonPropertyName("convention")]
    [Key("convention")]
    public Convention Convention { get; set; } = new();

    /// <summary>
    /// Required — MyPageTask.cs:110 constructs ArenaCompetition(responseData)
    /// which indexes data.competition_info.is_competition_period unconditionally
    /// (ArenaCompetition.cs:232-233). When false, the rest of the block is
    /// skipped, so a default-constructed CompetitionInfo is sufficient.
    /// </summary>
    [JsonPropertyName("competition_info")]
    [Key("competition_info")]
    public CompetitionInfo CompetitionInfo { get; set; } = new();

    // ── Battle / room recovery ─────────────────────────────────────────────

    /// <summary>Prod always sends concrete bool here even for fresh viewers — emit always.</summary>
    [JsonPropertyName("unfinished_battle_exists")]
    [Key("unfinished_battle_exists")]
    public bool UnfinishedBattleExists { get; set; }

    /// <summary>Only meaningful when UnfinishedBattleExists is true. Keep nullable + omitted otherwise — prod also omits it for fresh viewers.</summary>
    [JsonPropertyName("battle_finish_wait_time")]
    [Key("battle_finish_wait_time")]
    public int? BattleFinishWaitTime { get; set; }

    [JsonPropertyName("is_joined_room")]
    [Key("is_joined_room")]
    public bool IsJoinedRoom { get; set; }

    // ── Login bonus ────────────────────────────────────────────────────────

    [JsonPropertyName("can_give_daily_login_bonus")]
    [Key("can_give_daily_login_bonus")]
    public bool CanGiveDailyLoginBonus { get; set; }

    // ── User config (settings echo) ────────────────────────────────────────

    [JsonPropertyName("user_config")]
    [Key("user_config")]
    public UserConfig UserConfig { get; set; } = new();

    // ── Quest progress ─────────────────────────────────────────────────────

    [JsonPropertyName("quest")]
    [Key("quest")]
    public Quest Quest { get; set; } = new();

    /// <summary>
    /// Required — QuestOpenInfo.SetOpenInfo unconditionally calls .ToBoolean()
    /// on this root-level field (Wizard/QuestOpenInfo.cs:32). Omitting it would
    /// surface as a parse crash, not a defaulted value.
    /// </summary>
    [JsonPropertyName("is_hidden_boss_appeared")]
    [Key("is_hidden_boss_appeared")]
    public bool IsHiddenBossAppeared { get; set; }

    // ── Master Points season window ────────────────────────────────────────

    [JsonPropertyName("master_point_ranking_period")]
    [Key("master_point_ranking_period")]
    public MasterPointRankingPeriod MasterPointRankingPeriod { get; set; } = new();

    // ── Pre-release card preview ───────────────────────────────────────────

    /// <summary>Number cast to Prerelease.eStatus on the client.</summary>
    [JsonPropertyName("pre_release_status")]
    [Key("pre_release_status")]
    public int PreReleaseStatus { get; set; }

    // ── MyPage background ──────────────────────────────────────────────────

    [JsonPropertyName("user_mypage_info")]
    [Key("user_mypage_info")]
    public UserMyPageInfo UserMyPageInfo { get; set; } = new();

    // ── Basic puzzle badge ─────────────────────────────────────────────────

    [JsonPropertyName("basic_puzzle")]
    [Key("basic_puzzle")]
    public Common.BadgeFlag BasicPuzzle { get; set; } = new();

    // ── Battle Pass period flag ────────────────────────────────────────────

    /// <summary>
    /// Parsed by Data.ParseIsBattlePassPeriod. Same field as on /load/index
    /// (prod emits bool there too).
    /// </summary>
    [JsonPropertyName("is_battle_pass_period")]
    [Key("is_battle_pass_period")]
    public bool IsBattlePassPeriod { get; set; }

    // ── Special crystal info ───────────────────────────────────────────────

    /// <summary>
    /// Sibling under data, same shape as /load/index. Empty in the prod capture.
    /// </summary>
    [JsonPropertyName("special_crystal_info")]
    [Key("special_crystal_info")]
    public List<SpecialCrystalInfo> SpecialCrystalInfo { get; set; } = new();

    // ── Notification setters that index root-of-data directly ──────────────

    /// <summary>
    /// Required — ShopNotification.SetShopNotification indexes the four nested
    /// keys (card_pack, build_deck, sleeve, leader_skin) without TryGetValue
    /// (Wizard/ShopNotification.cs:33-37). The inner ShopAppealInfo ctor early-
    /// returns on empty, so default-constructed values are safe.
    /// </summary>
    [JsonPropertyName("shop_notification")]
    [Key("shop_notification")]
    public ShopNotification ShopNotification { get; set; } = new();

    /// <summary>
    /// Required — StoryNotification.SetStoryNotification indexes this key
    /// directly (Wizard/StoryNotification.cs:22) before applying GetValueOrDefault
    /// to its sub-fields.
    /// </summary>
    [JsonPropertyName("story_notification")]
    [Key("story_notification")]
    public StoryNotification StoryNotification { get; set; } = new();

    // ── Per-viewer / event state ───────────────────────────────────────────

    /// <summary>
    /// Full snapshot of the viewer's owned items — NOT a delta. The client's
    /// <c>MyPageTask.Parse</c> (line 155-163) clears <c>_userItemDict</c> the moment it sees
    /// this key, then re-populates from the wire list. Emitting <c>[]</c> wipes whatever
    /// /load/index populated, breaking any client logic that reads from the dict — most
    /// load-bearingly <c>PackChildGachaInfo.CostGoodsCount</c>, which gates tutorial-pack
    /// visibility via <c>PackConfig.EnableBuyPack</c>. Controllers MUST populate the full
    /// owned-items snapshot from <c>viewer.Items</c>; an empty list is correct only when the
    /// viewer genuinely owns nothing.
    /// </summary>
    [JsonPropertyName("user_item_list")]
    [Key("user_item_list")]
    public List<UserItem> UserItemList { get; set; } = new();

    [JsonPropertyName("gathering_info")]
    [Key("gathering_info")]
    public GatheringInfo GatheringInfo { get; set; } = new();

    /// <summary>Per-viewer offline-event participation. Empty for fresh viewers; prod also sends [].</summary>
    [JsonPropertyName("user_offline_event")]
    [Key("user_offline_event")]
    public List<object> UserOfflineEvent { get; set; } = new();

    // ── Fields prod sends as explicit null ─────────────────────────────────

    /// <summary>
    /// CRITICAL — emitting this field (even as null) routes MyPageTask.Parse through
    /// CampaignBattleWin.Clear() which initializes RewardList = new List&lt;...&gt;(). Without it,
    /// RewardList stays null and MyPageMenu.GetMyPageInfo NREs on its foreach iteration.
    /// See [[project-wire-null-policy]] for the broader "post-parse-consumer" rationale.
    /// </summary>
    [JsonPropertyName("treasure_info")]
    [Key("treasure_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object? TreasureInfo { get; set; }

    [JsonPropertyName("lottery_period_info")]
    [Key("lottery_period_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object? LotteryPeriodInfo { get; set; }

    [JsonPropertyName("all_card_enabled_period")]
    [Key("all_card_enabled_period")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public object? AllCardEnabledPeriod { get; set; }
}
