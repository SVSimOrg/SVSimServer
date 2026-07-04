using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Guild;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.Guild;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Infrastructure;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.MyPage;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.MyPage;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

public class MyPageController : SVSimController
{
    /// <summary>"yyyy-MM-dd HH:mm:ss" — prod's PHP convention. Used for wire-formatting DateTime
    /// columns that the client parses via DateTime.Parse on its side.</summary>
    private const string WireDateFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly IViewerRepository _viewerRepository;
    private readonly IGlobalsRepository _globalsRepository;
    private readonly IGameConfigService _config;
    private readonly IArenaTwoPickRunRepository _arenaTwoPickRuns;
    private readonly IHomeDialogSessionTracker _homeDialogTracker;
    private readonly ILoginBonusService _loginBonus;
    private readonly IGuildService _guild;
    private readonly IGuildInviteRepository _invites;
    private readonly IGuildJoinRequestRepository _joinRequests;
    private readonly IGuildChatMessageRepository _chat;

    public MyPageController(IViewerRepository viewerRepository, IGlobalsRepository globalsRepository,
        IGameConfigService config, IArenaTwoPickRunRepository arenaTwoPickRuns,
        IHomeDialogSessionTracker homeDialogTracker, ILoginBonusService loginBonus,
        IGuildService guild, IGuildInviteRepository invites,
        IGuildJoinRequestRepository joinRequests, IGuildChatMessageRepository chat)
    {
        _viewerRepository = viewerRepository;
        _globalsRepository = globalsRepository;
        _config = config;
        _arenaTwoPickRuns = arenaTwoPickRuns;
        _homeDialogTracker = homeDialogTracker;
        _loginBonus = loginBonus;
        _guild = guild;
        _invites = invites;
        _joinRequests = joinRequests;
        _chat = chat;
    }

    [HttpPost("index")]
    public async Task<ActionResult<MyPageIndexResponse>> Index(MyPageIndexRequest request)
    {
        var shortUdidClaim = User.Claims.FirstOrDefault(c => c.Type == ShadowverseClaimTypes.ShortUdidClaim)?.Value;
        if (shortUdidClaim is null || !long.TryParse(shortUdidClaim, out long shortUdid))
        {
            return Unauthorized();
        }

        Viewer? viewer = await _viewerRepository.GetViewerByShortUdid(shortUdid);
        if (viewer is null)
        {
            return NotFound();
        }

        var deviceHeader = Request.Headers["DEVICE"].FirstOrDefault();
        int deviceType = int.TryParse(deviceHeader, out int parsed) ? parsed : 0;

        // Hydrate all the globals slices in parallel-ish — they're independent reads.
        var rotation = _config.Get<RotationConfig>();
        var colosseumSeason = _config.Get<ColosseumSeasonConfig>();
        var sealedSeason = await _globalsRepository.GetCurrentSealedSeason();
        var masterPointPeriod = await _globalsRepository.GetCurrentMasterPointPeriod();
        var bannerEntries = await _globalsRepository.GetBanners();
        var specialDeckFormats = await _globalsRepository.GetActiveSpecialDeckFormats();
        var activeHomeDialogs = await _globalsRepository.GetActiveHomeDialogsAsync(DateTime.UtcNow);

        var homeDialogList = new List<Models.Dtos.Common.HomeDialog>();
        foreach (var entry in activeHomeDialogs)
        {
            if (_homeDialogTracker.TryReserve(viewer.ShortUdid, entry.Id))
            {
                homeDialogList.Add(BuildHomeDialog(entry));
                break;  // Client only reads [0]; emit at most one per call.
            }
        }

        // Remaining stubs are tagged TODO(mypage-stub) — see docs/api-spec/endpoints/post-login/mypage-index.md.
        return new MyPageIndexResponse
        {
            UserInfo = new UserInfo(deviceType, viewer),
            CanGiveDailyLoginBonus = _loginBonus.IsDue(viewer),
            UnreceivedMissionRewardCount = 0,                       // TODO(mypage-stub): viewer mission progress
            ReceiveFriendApplyCount = 0,                            // TODO(mypage-stub): viewer friend-request inbox
            UnreadPresentCount = await _viewerRepository.CountUnclaimedPresentsAsync(viewer.Id, HttpContext.RequestAborted),
            FriendBattleInviteCount = 0,                            // TODO(mypage-stub): viewer room-invite count
            GuildNotification = await BuildGuildNotificationAsync(viewer.Id, HttpContext.RequestAborted),
            LastAnnounceId = 0,                                     // TODO(mypage-stub): globals announcement metadata
            LastAnnounceUpdateTime = string.Empty,                  // TODO(mypage-stub): globals announcement metadata
            FeatureMaintenanceList = new(),                         // TODO(mypage-stub): FeatureMaintenanceEntry rows
            ArenaInfo = await BuildArenaInfosAsync(viewer.Id),
            IsArenaChallengePeriod = false,                         // TODO(mypage-stub): globals/ArenaSeason flag
            IsAvailableColosseumFreeEntry = false,                  // TODO(mypage-stub): viewer + globals free-entry quota
            ColosseumInfo = ColosseumLobbyInfoBuilder.Build(
                colosseumSeason, _config.Get<ColosseumRoundsConfig>(), DateTime.UtcNow),
            SealedInfo = BuildSealedInfo(sealedSeason),
            Banner = bannerEntries.Select(BuildBannerInfo).ToList(),
            RoomTypeInSession = new RoomTypeInSession
            {
                SpecialDeckFormatList = specialDeckFormats
                    .Select(e => new SpecialDeckFormat
                    {
                        DeckFormat = e.DeckFormat,
                        EndTime = e.EndTime.ToString(WireDateFormat, CultureInfo.InvariantCulture)
                    })
                    .ToList()
            },
            Convention = new Convention                             // TODO(mypage-stub): viewer offline-event participation
            {
                IsJoinTournament = false,
                IsAdminWatchUser = false,
            },
            UserConfig = new UserConfig
            {
                IsFoilPreferred = viewer.Info.IsFoilPreferred ? 1 : 0,
                IsPrizePreferred = viewer.Info.IsPrizePreferred ? 1 : 0,
                IsSkipGachaEffect = viewer.Info.IsSkipGachaEffect ? 1 : 0,
            },
            Quest = new Quest(),                                    // TODO(mypage-stub): active Quest event + viewer flags
            MasterPointRankingPeriod = BuildMasterPointRankingPeriod(masterPointPeriod),
            PreReleaseStatus = 0,                                   // TODO(mypage-stub): derive from PreReleaseInfo
            UserMyPageInfo = new UserMyPageInfo
            {
                UserMyPageSetting = new MyPageBgSetting
                {
                    MyPageId = viewer.MyPageBgId.ToString(),
                    SelectType = viewer.MyPageBgSelectType.ToString(),
                    MyPageIdList = viewer.MyPageBgRotation
                        .OrderBy(r => r.Slot)
                        .Select(r => r.BgId.ToString())
                        .ToList(),
                },
            },
            BasicPuzzle = new Models.Dtos.Common.BadgeFlag { IsDisplayBadge = false }, // TODO(mypage-stub): viewer practice-puzzle progress
            IsBattlePassPeriod = rotation.IsBattlePassPeriod,
            // The client's MyPageTask.Parse (line 155-163) does `_userItemDict.Clear();` whenever
            // user_item_list is present in the response — not when it's non-empty — and then
            // repopulates from the wire. Emitting [] here wipes the inventory the client populated
            // from /load/index, which makes PackChildGachaInfo.CostGoodsCount return 0 and filters
            // out is_hide=1 tutorial packs (the legendary starter 99047) via PackConfig.EnableBuyPack.
            // Populate from viewer.Items so the client's dict stays in sync with the DB.
            UserItemList = viewer.Items.Select(i => new UserItem(i)).ToList(),
            HomeDialogList = homeDialogList,
            SpecialCrystalInfo = new(),                             // TODO(mypage-stub): same shape/source as /load/index
            // CompetitionInfo, ShopNotification, StoryNotification, GuildNotification, GatheringInfo,
            // IsHiddenBossAppeared, SubBanner/SubBannerList/HomeDialogList/UserOfflineEvent/UserItemList,
            // and the three explicit-null fields (TreasureInfo, LotteryPeriodInfo, AllCardEnabledPeriod)
            // all rely on MyPageIndexResponse field initializers.
            // TODO(mypage-stub): wire competition_info from active tournament row (default false fine until tournaments exist).
            // TODO(mypage-stub): wire shop_notification from per-product shop-appeal state.
            // TODO(mypage-stub): wire story_notification from viewer story progress.
            // TODO(mypage-stub): wire is_hidden_boss_appeared from globals event flag.
            // TODO(mypage-stub): per-viewer state for user_item_list, gathering_info.is_entry, guild_notification, user_offline_event, home_dialog_list.
        };
    }

    /// <summary>
    /// Slim notification-delta endpoint — see MyPageRefreshResponse for the 3-field contract.
    /// Client fires this once after main-menu UI settles (and a second time shortly after; both
    /// calls get the same response). No state changes happen here; everything is read-only.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<MyPageRefreshResponse>> Refresh(MyPageRefreshRequest request)
    {
        var shortUdidClaim = User.Claims.FirstOrDefault(c => c.Type == ShadowverseClaimTypes.ShortUdidClaim)?.Value;
        if (shortUdidClaim is null || !long.TryParse(shortUdidClaim, out long shortUdid))
        {
            return Unauthorized();
        }

        Viewer? viewer = await _viewerRepository.GetViewerByShortUdid(shortUdid);
        if (viewer is null)
        {
            return NotFound();
        }

        return new MyPageRefreshResponse
        {
            FriendBattleInviteCount = 0,                           // TODO(mypage-stub): viewer room-invite count
            ShopNotification = new ShopNotification(),             // TODO(mypage-stub): per-product shop-appeal state
            GatheringNotification = new GatheringNotification(),   // empty matching message — correct for fresh viewers
        };
    }

    private async Task<GuildNotification> BuildGuildNotificationAsync(long viewerId, CancellationToken ct)
    {
        var view = await _guild.GetMyGuildAsync(viewerId, ct);
        var inviteCount = await _invites.CountPendingForInviteeAsync(viewerId, ct);
        var joinReqHasPending = (await _joinRequests.ListPendingForViewerAsync(viewerId, ct)).Count > 0;
        return new GuildNotification
        {
            GuildId = view?.Guild.GuildId,
            GuildRoomMessageId = view is null ? null : await _chat.GetMaxMessageIdSafelyAsync(view.Guild.GuildId, ct),
            IsJoinRequest = joinReqHasPending,
            IsInvited = inviteCount > 0,
        };
    }

    /// <summary>
    /// Mirrors LoadController.BuildArenaInfosAsync. /mypage/index has no Keys.Contains("arena_info")
    /// guard (ArenaData(jsonData["arena_info"]) at MyPageTask.cs:55 indexes [0] unconditionally), and
    /// the post-parse UI consumer (ChallengeEntry.SetChallengeInfo at ChallengeEntry.cs:35) reads
    /// _twoPickData.ChallengeData which is only built when arena_info[0].format_info is present.
    /// So we always populate format_info from the same ArenaSeason.FormatInfo jsonb /load/index uses.
    /// </summary>
    private async Task<List<ArenaInfo>> BuildArenaInfosAsync(long viewerId)
    {
        var season = await _globalsRepository.GetCurrentArenaSeason();

        // is_join MUST reflect the viewer's actual TK2 state — true iff they have an
        // active ViewerArenaTwoPickRun row. The client uses this to choose between the
        // "Pay to enter" and "Resume run" dialogs (Wizard/ChallengeEntry.cs:165 + ArenaEntryBase).
        // See LoadController.BuildArenaInfosAsync for the matching /load/index path.
        bool hasActiveRun = (await _arenaTwoPickRuns.GetByViewerIdAsync(viewerId)) is not null;

        if (season is null)
        {
            return new List<ArenaInfo>
            {
                new ArenaInfo
                {
                    Mode = 0,
                    Enable = 0,
                    Cost = 0,
                    RupeeCost = 0,
                    TicketCost = 0,
                    IsJoin = hasActiveRun,
                },
            };
        }

        ArenaFormatInfo? format = null;
        if (!string.IsNullOrEmpty(season.FormatInfo) && season.FormatInfo != "{}")
        {
            format = JsonSerializer.Deserialize<ArenaFormatInfo>(season.FormatInfo, JsonbReadOptions.Instance);
        }

        return new List<ArenaInfo>
        {
            new ArenaInfo
            {
                Mode = season.Mode,
                Enable = season.Enable,
                Cost = season.Cost,
                RupeeCost = season.RupyCost,
                TicketCost = season.TicketCost,
                IsJoin = hasActiveRun,
                FormatInfo = format,
            }
        };
    }

    private SealedInfo BuildSealedInfo(SealedConfig? row)
    {
        if (row is null) return new SealedInfo();

        List<int> packInfo = new();
        if (!string.IsNullOrEmpty(row.PackInfo) && row.PackInfo != "[]")
        {
            packInfo = JsonSerializer.Deserialize<List<int>>(row.PackInfo, JsonbReadOptions.Instance) ?? new();
        }

        SealedSalesPeriodInfo sales = new();
        if (!string.IsNullOrEmpty(row.SalesPeriodInfo) && row.SalesPeriodInfo != "{}")
        {
            sales = JsonSerializer.Deserialize<SealedSalesPeriodInfo>(row.SalesPeriodInfo, JsonbReadOptions.Instance)
                    ?? new SealedSalesPeriodInfo();
        }

        return new SealedInfo
        {
            Enable = row.Enable,
            CrystalCost = row.CrystalCost,
            RupyCost = row.RupyCost,
            TicketCost = row.TicketCost,
            IsJoin = row.IsJoin,
            PackInfo = packInfo,
            DeckUsingNumMin = row.DeckUsingNumMin,
            ScheduleId = row.ScheduleId,
            IsDeckCodeMaintenance = row.IsDeckCodeMaintenance,
            SalesPeriodInfo = sales,
        };
    }

    /// <summary>
    /// Deserializes the jsonb button_list column into wire-shape DTOs. Truncates >3 buttons —
    /// the client's switch in MyPageHomeDialog.InitializeButtonAction only handles 0/1/2/3,
    /// extras would be silently ignored anyway; doing it server-side keeps the wire honest.
    /// </summary>
    private static Models.Dtos.Common.HomeDialog BuildHomeDialog(HomeDialogEntry row)
    {
        List<Models.Dtos.Common.HomeDialogButtonDto> buttons = new();
        if (!string.IsNullOrEmpty(row.ButtonListJson) && row.ButtonListJson != "[]")
        {
            buttons = JsonSerializer.Deserialize<List<Models.Dtos.Common.HomeDialogButtonDto>>(
                row.ButtonListJson, JsonbReadOptions.Instance) ?? new();
        }
        if (buttons.Count > 3)
        {
            buttons = buttons.Take(3).ToList();
        }
        return new Models.Dtos.Common.HomeDialog
        {
            Type = row.Type?.ToString(CultureInfo.InvariantCulture),
            TitleTextId = row.TitleTextId,
            Image = row.Image,
            ButtonList = buttons,
        };
    }

    private static BannerInfo BuildBannerInfo(BannerEntry row)
    {
        List<string> imagePaths = new();
        if (!string.IsNullOrEmpty(row.ImagePaths) && row.ImagePaths != "[]")
        {
            imagePaths = JsonSerializer.Deserialize<List<string>>(row.ImagePaths, JsonbReadOptions.Instance) ?? new();
        }

        return new BannerInfo
        {
            ImageName = row.ImageName,
            Click = row.Click,
            Status = row.Status,
            // DB stores numeric, wire is string. PHP convention.
            ChangeTime = row.ChangeTime.ToString(CultureInfo.InvariantCulture),
            RemainingTime = row.RemainingTime.ToString(CultureInfo.InvariantCulture),
            ImagePaths = imagePaths,
        };
    }

    /// <summary>
    /// Far-future fallback EndTime so the client's DateTime.Parse(end_time) succeeds and
    /// Data.Load.data._masterResetNextTime gets seeded even when no globals row is present.
    /// </summary>
    private static MasterPointRankingPeriod BuildMasterPointRankingPeriod(MasterPointRankingPeriodEntry? row)
    {
        if (row is null)
        {
            return new MasterPointRankingPeriod
            {
                EndTime = "2030-01-01 00:00:00",
            };
        }

        return new MasterPointRankingPeriod
        {
            Id = row.Id,
            PeriodNum = row.PeriodNum,
            NecessaryScore = row.NecessaryScore,
            BeginTime = row.BeginTime.ToString(WireDateFormat, CultureInfo.InvariantCulture),
            EndTime = row.EndTime.ToString(WireDateFormat, CultureInfo.InvariantCulture),
        };
    }

    [HttpPost("finish_battle")]
    public ActionResult<MyPageFinishBattleResponse> FinishBattle([FromBody] MyPageFinishBattleRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();
        return new MyPageFinishBattleResponse { CheckUnfinishedBattle = 0 };
    }

    [HttpPost("get_special_crystal_info")]
    public ActionResult<EmptyResponse> GetSpecialCrystalInfo([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();
        // No special-crystal offers configured. Spec mock is `data: {}` for the no-offers
        // case — special_crystal_info field is optional and omitted when absent.
        return new EmptyResponse();
    }
}
