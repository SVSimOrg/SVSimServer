using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using PreReleaseInfoEntity = SVSim.Database.Models.PreReleaseInfo;
using PreReleaseInfoDto = SVSim.EmulatedEntrypoint.Models.Dtos.PreReleaseInfo;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Infrastructure;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

public class LoadController : SVSimController
{
    // Per-format rank entries the wire expects (5 entries, in deck_format discriminator order).
    // Hard-coded until viewer rank-state is persisted (see audit §6 #1).
    private static readonly Format[] RankFormats =
    {
        Format.Rotation, Format.Unlimited, Format.MyRotation, Format.Avatar, Format.Crossover
    };

    // Defense-in-depth: client unconditionally accesses RotationCardSetList[1] and [Count-1]
    // (LoadDetail.cs:184), so a list with < 2 entries crashes /load/index parsing. With both
    // CardImporter and per-domain seed importers run, the real list has ~6 entries. If something goes wrong
    // upstream (empty DB, bootstrap not yet run, etc.), fall back to this stub so the client at
    // least loads. Removing this is only safe once viewer-side bootstrap is unconditional.
    private static readonly List<CardSetIdentifier> StubRotationSets = new()
    {
        new CardSetIdentifier { SetId = 10000 },
        new CardSetIdentifier { SetId = 10005 },
        new CardSetIdentifier { SetId = 10010 }
    };

    private readonly IViewerRepository _viewerRepository;
    private readonly IGlobalsRepository _globalsRepository;
    private readonly IGameConfigService _config;
    private readonly IBattlePassService _battlePass;
    private readonly IViewerMissionStateService _missionState;
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;
    private readonly ICardMasterPayloadProvider _cardMaster;
    private readonly ILoginBonusService _loginBonus;

    public LoadController(IViewerRepository viewerRepository, IGlobalsRepository globalsRepository,
        IGameConfigService config,
        IBattlePassService battlePass, IViewerMissionStateService missionState,
        SVSimDbContext db, IInventoryService inv,
        ICardMasterPayloadProvider cardMaster,
        ILoginBonusService loginBonus)
    {
        _viewerRepository = viewerRepository;
        _globalsRepository = globalsRepository;
        _config = config;
        _battlePass = battlePass;
        _missionState = missionState;
        _db = db;
        _inv = inv;
        _cardMaster = cardMaster;
        _loginBonus = loginBonus;
    }

    [HttpPost("index")]
    public async Task<ActionResult<IndexResponse>> Index(IndexRequest request, CancellationToken ct)
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

        // Backfill any card-associated cosmetics the viewer should already own. Idempotent.
        // We MUST re-fetch the viewer after this call because GetViewerByShortUdid uses
        // .AsNoTracking() — the local `viewer` instance is detached, and the service's writes
        // (on a separate tracked instance) won't appear on this snapshot. Without the re-fetch,
        // the response payload would be one /load/index behind on newly-granted cosmetics.
        await using var tx = await _inv.BeginAsync(viewer.Id, ct);
        await tx.BackfillCardCosmeticsAsync(ct);
        DailyLoginBonus? loginBonusDto = await _loginBonus.GrantIfDueAsync(tx, ct);
        await tx.CommitAsync(ct);

        // Lazy-materialize mission/achievement state. Idempotent — safe to call every /load/index.
        await _missionState.EnsureCurrentAsync(viewer.Id);
        await _db.SaveChangesAsync();

        viewer = await _viewerRepository.GetViewerByShortUdid(shortUdid);
        if (viewer is null)
        {
            return NotFound();   // defensive — should never happen
        }

        // user_card_list policy (see docs/api-spec/endpoints/post-login/load-index.md
        // §user_card_list for the full discussion):
        //
        //   We emit ONLY cards the viewer actually owns (Count > 0), plus basics — which
        //   the client treats as always-3-of, protected (un-disenchantable).
        //
        // Prod returns a larger, curated set (~1k entries) that includes some 0-count
        // "ever-touched" rows from the viewer's collection history (cards they've owned
        // and since disenchanted, or cards in card-sets they've engaged with). We don't
        // model "cards ever owned" today, so we can't reproduce that exactly. The client
        // tolerates the divergence: GetUserOwnCardData() builds a dict keyed by card_id
        // and falls back to 0 for any absent id (DataMgr.cs:1182), so "absent" and
        // "Count=0" are semantically interchangeable for lookups, deck construction, and
        // craft-cost queries.
        //
        // The UI difference would show up only in views that iterate UserCardList
        // *directly* to enumerate "cards I've held" (e.g. some collection-screen filters).
        // To close that gap later, see the "user_card_list — closer-to-prod options"
        // section of the spec doc: Option B (union with active-rotation card-set
        // members at Count=0) is the cheapest upgrade; Option C requires a new
        // ever-touched flag on OwnedCardEntry.
        //
        // Filters always applied — these are noise in prod too:
        //   * IsResurgentCard rows: prod returns zero of these
        //   * card_set_id=90000 (engine tokens, char_type=4): never collectible
        // Both naturally fall out of "ownership-only" since the viewer can't own them;
        // re-confirm the filter if we later move to Option B and start iterating card-sets.
        // Owned-card projection (incl. the freeplay "all cards" path) lives in the entitlements
        // service so both modes share one definition.
        var allCardsAsOwned = await _inv.EffectiveOwnedCardsAsync(viewer, ct);

        var cosmetics = await _inv.EffectiveCosmeticsAsync(viewer, ct);
        var classExpCurve = await _globalsRepository.GetClassExpCurve();

        List<ClassExp> classExps = new();
        int accumulateExp = 0;
        int? prevNecessaryExp = null;
        foreach (var entry in classExpCurve)
        {
            accumulateExp += entry.NecessaryExp;
            classExps.Add(new ClassExp
            {
                Level = entry.Id,
                NecessaryExp = entry.NecessaryExp,
                DiffExp = prevNecessaryExp.HasValue ? entry.NecessaryExp - prevNecessaryExp.Value : entry.NecessaryExp,
                AccumulateExp = accumulateExp
            });
            prevNecessaryExp = entry.NecessaryExp;
        }

        // Globals — one cached fetch per slice. The Rotation/Challenge/DefaultLoadout sections
        // come via IGameConfigService (DB → appsettings → ShippedDefaults). Other repo methods
        // come from the per-domain seed importers in SVSim.Bootstrap.
        var rotation = _config.Get<RotationConfig>();
        var challenge = _config.Get<ChallengeConfig>();
        var defaultLoadout = _config.Get<DefaultLoadoutConfig>();

        List<CardSetIdentifier> rotationSets = (await _globalsRepository.GetRotationCardSets())
            .OrderBy(s => s.Id)
            .Select(set => new CardSetIdentifier { SetId = set.Id })
            .ToList();
        if (rotationSets.Count < 2) rotationSets = StubRotationSets;

        var deviceHeader = Request.Headers["DEVICE"].FirstOrDefault();
        int deviceType = int.TryParse(deviceHeader, out int parsed) ? parsed : 0;

        var response = new IndexResponse
        {
            UserTutorial = new UserTutorial { TutorialStep = viewer.MissionData.TutorialState },
            UserInfo = new UserInfo(deviceType, viewer),
            UserCurrency = new UserCurrency(viewer)
            {
                Crystals = (ulong)_inv.EffectiveBalance(viewer, SpendCurrency.Crystal),
                TotalCrystals = (ulong)_inv.EffectiveBalance(viewer, SpendCurrency.Crystal),
                Rupees = (ulong)_inv.EffectiveBalance(viewer, SpendCurrency.Rupee),
                RedEther = (ulong)_inv.EffectiveBalance(viewer, SpendCurrency.RedEther),
            },
            UserItems = viewer.Items.Select(item => new UserItem(item)).ToList(),
            SpotPoint = checked((int)viewer.Currency.SpotPoints),
            UserRotationDecks = new UserFormatDeckInfo
            {
                UserDecks = viewer.Decks.Where(d => d.Format == Format.Rotation)
                    .Select(d => new UserDeck(d)).ToList()
            },
            UserUnlimitedDecks = new UserFormatDeckInfo
            {
                UserDecks = viewer.Decks.Where(d => d.Format == Format.Unlimited)
                    .Select(d => new UserDeck(d)).ToList()
            },
            UserMyRotationDecks = new UserFormatDeckInfo
            {
                UserDecks = viewer.Decks.Where(d => d.Format == Format.MyRotation)
                    .Select(d => new UserDeck(d)).ToList()
            },
            UserCards = allCardsAsOwned.Select(card => new UserCard(card)).ToList(),
            UserClasses = viewer.Classes.Select(vc => new UserClass(
                vc,
                viewer.LeaderSkins.Where(s => s.ClassId == vc.Class.Id).Select(s => s.Id).ToList())).ToList(),
            Sleeves = cosmetics.SleeveIds.Select(id => new SleeveIdentifier { SleeveId = id }).ToList(),
            UserEmblems = cosmetics.EmblemIds.Select(id => new EmblemIdentifier { EmblemId = id }).ToList(),
            UserDegrees = cosmetics.DegreeIds.Select(id => new DegreeIdentifier { DegreeId = id }).ToList(),
            LeaderSkins = cosmetics.AllLeaderSkins
                .Select(skin => new UserLeaderSkin(skin, cosmetics.OwnedLeaderSkinIds.Contains(skin.Id)))
                .ToList(),
            MyPageBackgrounds = cosmetics.MyPageBackgroundIds.Select(id => id.ToString()).ToList(),
            LootBoxRegulations = new LootBoxRegulations(),
            GatheringInfo = new GatheringInfo(),
            IsBattlePassPeriod = rotation.IsBattlePassPeriod,
            BattlePassLevelInfo = await _battlePass.GetLevelCurveAsync(ct),
            SpecialCrystalInfos = new List<SpecialCrystalInfo>(),
            AvatarRotationInfo = await BuildAvatarInfoAsync(),
            MyRotationInfo = await BuildMyRotationInfoAsync(),
            // Prod 2026-05-23 emits `[]`; FeatureMaintenanceEntry table is skeleton-seeded for the
            // same reason. When a real maintenance window is captured we'll learn the wire shape of
            // each entry (the existing FeatureMaintenance enum maps to feature_id but the wrapping
            // object's other fields are TBD — see audit Open Questions).
            FeatureMaintenances = new List<FeatureMaintenance>(),
            PreReleaseInfo = await BuildPreReleaseInfoAsync(),
            SpotCards = (await _globalsRepository.GetSpotCards())
                .ToDictionary(e => e.Id.ToString(), e => e.Cost),
            ReprintedCards = (await _globalsRepository.GetReprintedCards())
                .Select(e => e.Id).ToList(),
            UnlimitedBanList = (await _globalsRepository.GetUnlimitedRestrictions())
                .ToDictionary(e => e.Id.ToString(), e => e.RestrictionValue),
            LoadingTipCardExclusions = (await _globalsRepository.GetLoadingExclusionCards())
                .Select(e => e.Id).ToList(),
            MaintenanceCards = (await _globalsRepository.GetMaintenanceCards())
                .Select(e => e.Id).ToList(),
            RedEtherOverrides = new List<RedEtherOverride>(),
            DailyLoginBonus = loginBonusDto,
            UserRankedMatches = new List<UserRankedMatches>(),
            UserRankInfo = RankFormats.Select(f => new UserRankInfo
            {
                DeckFormat = f,
                Rank = 1,
                BattlePoints = 0,
                WinStreak = 0,
                IsPromotion = 0,
                IsMasterRank = 0,
                IsGrandMasterRank = 0,
                MasterPoints = 0
            }).ToList(),
            ArenaConfig = new ArenaConfig
            {
                UseChallengePickTwoPremiumCard = viewer.Info.UseChallengeTwoPickPremiumCard ? 1 : 0,
                ChallengePickTwoCardSleeve = (int)(viewer.Info.ChallengeTwoPickSleeveId != 0
                    ? viewer.Info.ChallengeTwoPickSleeveId
                    : defaultLoadout.SleeveId),
            },
            ArenaInfos = await BuildArenaInfosAsync(viewer.Id),
            RotationSets = rotationSets,
            UserConfig = new UserConfig
            {
                IsFoilPreferred = viewer.Info.IsFoilPreferred ? 1 : 0,
                IsPrizePreferred = viewer.Info.IsPrizePreferred ? 1 : 0,
                IsSkipGachaEffect = viewer.Info.IsSkipGachaEffect ? 1 : 0,
            },
            OpenBattlefieldIds = (await _globalsRepository.GetBattlefields(true))
                .Select(bf => bf.Id.ToString()).ToList(),
            DefaultSettings = new DefaultSettings(defaultLoadout),
            ClassExp = classExps,
            RankInfo = (await _globalsRepository.GetRankInfo()).Select(ri => new RankInfo(ri)).ToList(),
            DeckFormat = Format.Rotation,
            CardSetIdForResourceDlView = rotation.CardSetIdForResourceDlView,
        };

        // Emit card_master_hash only when the client's local copy differs from the configured
        // hash (presence-only client check — Wizard/CardMaster.cs:20). Emitting on every boot
        // would force a 1.27 MB redownload every boot. Empty request hash = fresh client = mismatch.
        if (_cardMaster.IsAvailable)
        {
            var cardMasterCfg = _config.Get<CardMasterConfig>();
            if (cardMasterCfg.EnableServing &&
                !string.Equals(request.CardMasterHash, cardMasterCfg.CurrentHash, StringComparison.Ordinal))
            {
                response.CardMasterHash = cardMasterCfg.CurrentHash;
            }
        }

        return response;
    }

    /// <summary>
    /// Builds <c>arena_info</c> as the single-element array the client's ArenaData(JsonData[0]) ctor
    /// expects (audit §1). Returns null when no current Take Two season is seeded — the IndexResponse
    /// field is omitted on the wire, which the client's <c>Keys.Contains("arena_info")</c> guard
    /// (LoadDetail.cs:261) handles cleanly.
    /// </summary>
    private async Task<List<ArenaInfo>?> BuildArenaInfosAsync(long viewerId)
    {
        var season = await _globalsRepository.GetCurrentArenaSeason();
        if (season is null) return null;

        ArenaFormatInfo? format = null;
        if (!string.IsNullOrEmpty(season.FormatInfo) && season.FormatInfo != "{}")
        {
            format = JsonSerializer.Deserialize<ArenaFormatInfo>(season.FormatInfo, JsonbReadOptions.Instance);
        }

        // is_join must reflect the viewer's actual TK2 state — true if they have an
        // active ViewerArenaTwoPickRun row. The client uses this to decide between the
        // "Pay to enter" and "Resume run" dialogs (Wizard/ChallengeEntry.cs:165 + ArenaEntryBase).
        // Without a per-viewer override here, every cold start after a partial run shows
        // "Pay to enter" — losing the in-progress draft from the player's perspective.
        bool hasActiveRun = await _db.ViewerArenaTwoPickRuns
            .AsNoTracking()
            .AnyAsync(r => r.ViewerId == viewerId);

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

    /// <summary>
    /// Builds <c>my_rotation_info</c> from the joined MyRotationSettingEntry + MyRotationAbilityEntry
    /// tables. Each setting row's ReprintedCardIds / RestrictedCardIds jsonb is parsed back to the
    /// dict shape the client expects.
    /// </summary>
    private async Task<MyRotationInfo?> BuildMyRotationInfoAsync()
    {
        var settings = await _globalsRepository.GetMyRotationSettings();
        var abilities = await _globalsRepository.GetMyRotationAbilities();
        if (settings.Count == 0 && abilities.Count == 0) return null;

        return new MyRotationInfo
        {
            Settings = settings.ToDictionary(
                s => s.Id.ToString(),
                s => new SpecialRotationSetting
                {
                    RotationId = s.Id,
                    CardSetIds = s.CardSetIdsCsv,
                    Abilities = s.AbilitiesCsv,
                }),
            Abilities = abilities.ToDictionary(
                a => a.Id.ToString(),
                a => JsonSerializer.Deserialize<MyRotationAbility>(a.Data, JsonbReadOptions.Instance) ?? new MyRotationAbility()),
            ReprintedCards = settings.ToDictionary(
                s => s.Id.ToString(),
                s => JsonSerializer.Deserialize<Dictionary<string, int>>(s.ReprintedCardIds, JsonbReadOptions.Instance) ?? new()),
            Banlist = settings.ToDictionary(
                s => s.Id.ToString(),
                s => JsonSerializer.Deserialize<Dictionary<string, int>>(s.RestrictedCardIds, JsonbReadOptions.Instance) ?? new()),
            DisabledCardSets = new List<int>(), // prod 2026-05-23 emits empty list; refine if/when populated
            Schedules = BuildMyRotationSchedules(),
        };
    }

    /// <summary>
    /// Maps the <c>MyRotationSchedule</c> config section to the wire-shape <c>SpecialRotationSchedule</c>.
    /// The client gates the Custom Rotation format-selector button on <c>FreeBattle</c>'s window
    /// being currently open (Wizard/MyRotationAllInfo.cs:45), so a default-initialised
    /// <c>DateTime.MinValue</c> pair here hides the button. Config defaults reproduce the
    /// 2026-05-23 prod capture; the rotation-config seed file overwrites from newer captures.
    /// </summary>
    private SpecialRotationSchedule BuildMyRotationSchedules()
    {
        var cfg = _config.Get<MyRotationScheduleConfig>();
        return new SpecialRotationSchedule
        {
            Gathering = new DateRange { BeginTime = cfg.Gathering.Begin, EndTime = cfg.Gathering.End },
            FreeBattle = new DateRange { BeginTime = cfg.FreeBattle.Begin, EndTime = cfg.FreeBattle.End },
        };
    }

    /// <summary>
    /// Builds <c>avatar_info</c> from AvatarAbilityEntry rows. Schedules is an empty list per the
    /// 2026-05-23 prod capture (active Avatar windows would populate it; entry shape TBD).
    /// </summary>
    private async Task<AvatarInfo?> BuildAvatarInfoAsync()
    {
        var abilities = await _globalsRepository.GetAvatarAbilities();
        if (abilities.Count == 0) return null;

        return new AvatarInfo
        {
            Abilities = abilities.ToDictionary(
                a => a.Id.ToString(),
                a => new AvatarAbility
                {
                    LeaderSkinId = a.LeaderSkinId,
                    BattleStartFirstPlayerBp = a.BattleStartFirstPlayerTurnBp,
                    BattleStartSecondPlayerBp = a.BattleStartSecondPlayerTurnBp,
                    BattleStartMaxLife = a.BattleStartMaxLife,
                    AbilityCost = a.AbilityCost,
                    Ability = a.Ability,
                    PassiveAbility = a.PassiveAbility,
                    AbilityDesc = a.AbilityDesc,
                    PassiveAbilityDesc = a.PassiveAbilityDesc,
                }),
            Schedules = new List<AvatarSchedule>(),
        };
    }

    /// <summary>
    /// Builds <c>pre_release_info</c> from the singleton PreReleaseInfo entity. Returns null when
    /// the entity is absent. NB: the 2026-05-23 prod capture had stale 1900/2019 dates which the
    /// audit flagged as the "no active pre-release" sentinel — we emit them as-is rather than
    /// hiding the field, because that's what prod itself does.
    /// </summary>
    private async Task<PreReleaseInfoDto?> BuildPreReleaseInfoAsync()
    {
        var pri = await _globalsRepository.GetPreReleaseInfo();
        if (pri is null) return null;

        return new PreReleaseInfoDto
        {
            Id = pri.PreReleaseId,
            StartTime = pri.StartTime,
            EndTime = pri.EndTime,
            DisplayEndTime = pri.DisplayEndTime,
            NextCardSetId = pri.NextCardSetId,
            DefaultCardMasterId = pri.DefaultCardMasterId,
            PreReleaseCardMasterId = pri.PreReleaseCardMasterId,
            FreeMatchStartTime = pri.FreeMatchStartTime,
            CardMasterId = pri.CardMasterId,
            RotationCardSets = JsonSerializer.Deserialize<List<int>>(pri.RotationCardSetIdList, JsonbReadOptions.Instance) ?? new(),
            ReprintedCardIds = JsonSerializer.Deserialize<Dictionary<string, string>>(pri.ReprintedBaseCardIds, JsonbReadOptions.Instance) ?? new(),
            LatestReprintedCardIds = JsonSerializer.Deserialize<List<int>>(pri.LatestReprintedBaseCardIds, JsonbReadOptions.Instance) ?? new(),
            IsPreRotationFreeMatchTerm = pri.IsPreRotationFreeMatchTerm ? 1 : 0,
        };
    }
}
