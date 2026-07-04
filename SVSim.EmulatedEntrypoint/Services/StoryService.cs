using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using SVSim.Database;
using SVSim.Database.Entities.Story;
using SVSim.Database.Enums;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.BuildDeck;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;
using SVSim.Database.Services.Inventory;
using SVSim.Database.Repositories.Story;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Story;

namespace SVSim.EmulatedEntrypoint.Services;

public class StoryService : IStoryService
{
    private readonly IStoryMasterRepository _master;
    private readonly IViewerStoryProgressRepository _viewer;
    private readonly IInventoryService _inv;
    private readonly SVSimDbContext _db;
    private readonly IGameConfigService _configService;
    private readonly IDeckRepository _deckRepository;
    private readonly IBuildDeckRepository _buildDecks;
    private readonly IViewerRepository _viewers;
    private readonly IBattleXpService _xp;
    private readonly ILogger<StoryService> _logger;

    public StoryService(
        IStoryMasterRepository master,
        IViewerStoryProgressRepository viewer,
        IInventoryService inv,
        SVSimDbContext db,
        IGameConfigService configService,
        IDeckRepository deckRepository,
        IBuildDeckRepository buildDecks,
        IViewerRepository viewers,
        IBattleXpService xp,
        ILogger<StoryService> logger)
    {
        _master = master;
        _viewer = viewer;
        _inv = inv;
        _db = db;
        _configService = configService;
        _deckRepository = deckRepository;
        _buildDecks = buildDecks;
        _viewers = viewers;
        _xp = xp;
        _logger = logger;
    }

    public async Task<InfoResponse> GetInfoAsync(StoryApiType apiType, int sectionId, int? charaId, long viewerId)
    {
        var resolvedChara = charaId ?? 0;
        var chapters = await _master.GetChaptersBySectionCharaAsync(sectionId, resolvedChara);
        if (chapters.Count == 0)
            return new InfoResponse();

        // Include sub-chapter story_ids in the progress lookup — they're independent progress
        // markers (each sub vignette gets its own ViewerStoryProgress row) and feed the per-sub
        // is_finish flag in the response.
        var storyIds = chapters.Select(c => c.StoryId)
                               .Concat(chapters.SelectMany(c => c.SubChapters).Select(sc => sc.SubChapterStoryId))
                               .Distinct()
                               .ToList();
        // Sequential awaits — both repos share the scoped DbContext, and EF Core forbids
        // concurrent operations on a single context. Parallel Task.WhenAll throws
        // InvalidOperationException ("A second operation was started on this context...").
        var progress = await _viewer.GetProgressForChaptersAsync(viewerId, storyIds);
        var unlocked = await _viewer.GetBranchUnlockedStoryIdsAsync(viewerId, storyIds);

        var byChapterId = chapters.ToDictionary(c => c.ChapterId);
        var resp = new InfoResponse();

        foreach (var c in chapters.OrderBy(x => ChapterRowNum(x.ChapterId))
                                  .ThenBy(x => x.ChapterId, StringComparer.Ordinal))
        {
            var parent = chapters.FirstOrDefault(p =>
                !ReferenceEquals(p, c) &&
                p.NextChapterId.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(c.ChapterId));

            // A chapter is a "branch child" only at the SPLIT point — where the parent declares
            // multiple successors (e.g. ch2.next="3a 3b 3c"). The alphabetic suffix is inherited
            // across the rest of the branched path (3a→4a→5a→...) but only ch3a/3b/3c carry the
            // explicit unlock gate; downstream "4a"/"4b" are normal single successors. Suffix-based
            // detection (^\d+[a-z]+) wrongly tagged every "4a"-style chapter as a branch child.
            bool isBranchChild = parent is not null
                && parent.NextChapterId.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1;

            // is_released = "chapter visible in the section UI" — gated on parent progress.
            // For branch children this is the PARENT's finish state, NOT whether THIS branch
            // was selected — siblings of the chosen branch must still appear so the player
            // sees the alternative paths exist (rendered locked). Verified against prod
            // traffic_prod_haven_choices.ndjson lines 22/28/34 where 3a/3b/3c all carry
            // is_released=true regardless of which branch was previously chosen.
            bool released = parent is null
                || (progress.TryGetValue(parent.StoryId, out var pp) && (pp.IsFinish || pp.IsSkipped));

            // Optional required_chapter_id gate (additional release condition only).
            if (!string.IsNullOrEmpty(c.RequiredChapterId) &&
                byChapterId.TryGetValue(c.RequiredChapterId, out var req))
            {
                bool reqDone = progress.TryGetValue(req.StoryId, out var rp)
                               && (rp.IsFinish || rp.IsSkipped);
                released = released && reqDone;
            }

            // is_lock = "chapter has an explicit gate not yet satisfied" — INDEPENDENT of
            // is_released. The only gate in the current catalog is the branch-sibling
            // selection: unselected branch children carry is_lock=true even though they
            // remain visible. Non-branch chapters never carry an implicit lock; their
            // availability is communicated entirely through is_released.
            bool locked = isBranchChild && !unlocked.Contains(c.StoryId);

            var pState = progress.GetValueOrDefault(c.StoryId);

            resp.StoryMasterList.Add(new StoryMasterEntry
            {
                StoryId = c.StoryId.ToString(),
                SectionId = c.SectionId.ToString(),
                CharaId = c.CharaId.ToString(),
                ChapterId = c.ChapterId,
                NextChapterId = c.NextChapterId,
                RequiredChapterId = c.RequiredChapterId ?? "",
                SelectionDisplayPosition = c.SelectionDisplayPosition ?? "",
                SelectionTextId = c.SelectionTextId ?? "",
                ShowCoordinate = c.ShowCoordinate.ToString(),
                XCoordinate = c.XCoordinate.ToString("0.#####"),
                YCoordinate = c.YCoordinate.ToString("0.#####"),
                IsCameraMovable = c.IsCameraMovable.ToString(),
                ShowSubtitles = c.ShowSubtitles.ToString(),
                BattleExists = c.BattleExists,
                EnemyCharaId = c.EnemyCharaId.ToString(),
                EnemyClass = c.EnemyClass.ToString(),
                EnemyAiId = c.EnemyAiId.ToString(),
                BgFileName = c.BgFileName,
                ChapterEffectPath = c.ChapterEffectPath ?? "",
                ChapterClearTextId = c.ChapterClearTextId ?? "",
                Battle3dFieldId = c.Battle3dFieldId.ToString(),
                BgmId = c.BgmId,
                SpecialBattleSettingId = c.SpecialBattleSettingId?.ToString() ?? "",
                ReleasePoint = c.ReleasePoint.ToString(),
                BattleSettings = c.BattleSettings.Select(b => new BattleSettingDto
                {
                    DeckClassId = b.DeckClassId,
                    PlayerEmotionOverride = b.PlayerEmotionOverride,
                    EnemyEmotionOverride = b.EnemyEmotionOverride,
                    SkinIdOverride = b.SkinIdOverride,
                    Battle3dFieldIdOverride = b.Battle3dFieldIdOverride,
                    BgmIdOverride = b.BgmIdOverride,
                    DeckSkinIdOverride = b.DeckSkinIdOverride,
                }).ToList(),
                StoryReward = c.Rewards.Select(r => new RewardDto
                {
                    RewardType = ((int)r.RewardType).ToString(),
                    RewardDetailId = r.RewardDetailId.ToString(),
                    RewardNumber = r.RewardNumber.ToString(),
                }).ToList(),
                SubChapters = c.SubChapters.Count == 0
                    ? null
                    : c.SubChapters.Select(sc => new SubChapterDto
                    {
                        StoryId = sc.SubChapterStoryId,
                        SubChapterId = sc.SubChapterId,
                        IsFinish = progress.TryGetValue(sc.SubChapterStoryId, out var sp) && sp.IsFinish,
                        IsMaintenanceChapter = sc.IsMaintenanceChapter,
                    }).ToList(),
                IsMaintenanceChapter = c.IsMaintenanceChapter,
                IsReleased = released,
                IsLock = locked,
                UnlockText = c.UnlockText ?? "",
                IsSkipped = pState?.IsSkipped ?? false,
                IsFinish = pState?.IsFinish ?? false,
                IsPlayAnotherEndAppearanceAnimation = c.IsPlayAnotherEndAppearanceAnimation,
                IsReleasedAnotherEnd = c.IsReleasedAnotherEnd,
                // TODO: prod gates skip on tutorial chapters specifically — the first battle of
                // each class's section-1 intro (the "class tutorial" chapters) only shows skip on
                // REPLAY, not on first play. Other chapters honor the chapter-master flag as-is.
                // Our captures are all post-clear so the exact gate is unconfirmed; cosmetic-only,
                // viewer sees skip earlier than prod would allow on class-tutorial first plays.
                IsSkipEnabled = c.IsSkipEnabled,
            });
        }

        return resp;
    }

    public async Task<SectionResponse> GetSectionsAsync(StoryApiType apiType, long viewerId)
    {
        var sections = await _master.GetSectionsByFamilyAsync(apiType);
        if (sections.Count == 0) return new SectionResponse();

        var worldIds = sections.Where(s => s.WorldId.HasValue).Select(s => s.WorldId!.Value).Distinct().ToList();

        // Four bulk loads total — no per-(section,chara) round-trips. For a full main-story sweep
        // this is 4 queries instead of ~336. Sequential (not Task.WhenAll) because both repos
        // share the scoped DbContext — EF Core forbids concurrent operations on a single context.
        var worlds = await _master.GetWorldsForSectionsAsync(worldIds);
        var sectionIds = sections.Select(s => s.Id).ToList();
        var allChapters = await _master.GetChaptersBySectionsAsync(sectionIds);

        var allProgress = await _viewer.GetProgressForChaptersAsync(
            viewerId, allChapters.Select(c => c.StoryId));

        // Tutorial section (id=0) has no chapter rows server-side — the prologue is hardcoded
        // client-side in Wizard/Prologue.cs. Derive its is_finished from viewer.tutorial_step
        // instead (matches prod traffic_prod_626_story.ndjson btn_story_tutorial). The client
        // uses is_finished to flip IsTutorialReplay, which is what re-enables chapter switching
        // in AreaSelectUI.OnTouchChapterListTutorial when the user re-visits the prologue.
        const int TutorialEndStep = 100;
        var tutorialState = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .Select(v => v.MissionData.TutorialState)
            .FirstOrDefaultAsync();

        // Index chapters by (sectionId, charaId) for O(1) lookup in the rollup loop.
        var chaptersBySectionChara = allChapters
            .GroupBy(c => (c.SectionId, c.CharaId))
            .ToDictionary(g => g.Key, g => g.ToList());

        var charaIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var resp = new SectionResponse();

        foreach (var w in worlds)
        {
            var sectionsInWorld = sections.Where(s => s.WorldId == w.Id).OrderBy(s => s.OrderId).ToList();
            var worldDto = new SectionWorld
            {
                TitleTextId = w.TitleTextKey,
                PanelImageName = w.PanelImageName,
                RibbonText = w.RibbonText,
            };
            bool worldComplete = sectionsInWorld.Count > 0;
            foreach (var s in sectionsInWorld)
            {
                int released = 0, finished = 0;
                bool sectionFinished;

                if (s.IsLeaderSelect)
                {
                    // released_chara_count = charas with playable chapters in the catalog
                    // (chapter 1 is always unlocked, so a chara is "released" the moment it has
                    // any chapter row). Drives the "X/Y complete" label, which the client only
                    // renders when released > 0. Counter is per-catalog, NOT per-viewer-progress.
                    // finished_chara_count = charas where the viewer has cleared every chapter.
                    int charasWithChapters = 0;
                    foreach (var c in charaIds)
                    {
                        if (!chaptersBySectionChara.TryGetValue((s.Id, c), out var chapters) || chapters.Count == 0)
                            continue;
                        charasWithChapters++;
                        int doneCount = chapters.Count(x =>
                            allProgress.TryGetValue(x.StoryId, out var p) && (p.IsFinish || p.IsSkipped));
                        if (doneCount == chapters.Count) finished++;
                    }
                    released = charasWithChapters;
                    sectionFinished = released > 0 && finished == released;
                }
                else
                {
                    // Non-leader-select sections (Limited / Event story) use chara_id=0 and don't
                    // expose chara counters — prod emits released=finished=0 regardless of progress.
                    // is_finished is derived from completion of the single chara=0 chapter set.
                    chaptersBySectionChara.TryGetValue((s.Id, 0), out var chapters);
                    if (chapters is { Count: > 0 })
                    {
                        int doneCount = chapters.Count(x =>
                            allProgress.TryGetValue(x.StoryId, out var p) && (p.IsFinish || p.IsSkipped));
                        sectionFinished = doneCount == chapters.Count;
                    }
                    else
                    {
                        sectionFinished = false;
                    }
                }

                if (s.Id == 0) sectionFinished = tutorialState >= TutorialEndStep;

                if (!sectionFinished) worldComplete = false;
                worldDto.SectionList.Add(new SectionEntry
                {
                    SectionId = s.Id.ToString(),
                    OrderId = s.OrderId,
                    AllStoryOrderId = s.AllStoryOrderId.ToString(),
                    Name = s.NameTextKey,
                    ImageName = s.ImageName,
                    IsLeaderSelect = s.IsLeaderSelect,
                    BackGroundId = s.BackGroundId,
                    IsFinished = sectionFinished,
                    ReleasedCharaCount = released,
                    FinishedCharaCount = finished,
                    IsUnderMaintenance = s.IsUnderMaintenance,
                    ChapterSelectType = s.ChapterSelectType.ToString(),
                    StoryTypeOverwrite = s.StoryTypeOverwrite.ToString(),
                    IsNew = false,
                    IsPlayAnotherEndAppearanceAnimation = s.IsPlayAnotherEndAppearanceAnimation,
                    IsSpoiler = s.IsSpoiler,
                    SpoilerMessage = s.SpoilerMessage,
                });
            }
            worldDto.IsComplete = worldComplete;
            resp.WorldList[w.Id.ToString()] = worldDto;
        }
        return resp;
    }
    public async Task<LeaderSelectResponse> GetLeaderSelectAsync(StoryApiType apiType, int sectionId, long viewerId)
    {
        // Leader list comes from whatever chara_ids the section's chapter catalog actually contains —
        // NOT a fixed 1..8 enumeration. Sections in prod range from standard class subsets
        // (e.g. section 5: charas 3,5,6,8) to custom-leader sections (e.g. section 17: chara_ids
        // 500901-500904). Order is by ascending min(story_id) per chara, which reproduces prod's
        // ordering for every captured section (including section 17's 500901,500903,500904,500902
        // and section 15's 500701,500732,500704). Verified against traffic_prod_626_story.ndjson.
        var chapters = await _master.GetChaptersBySectionsAsync(new[] { sectionId });
        var charaGroups = chapters
            .GroupBy(c => c.CharaId)
            .Select(g => (CharaId: g.Key, Chapters: g.ToList(), MinStoryId: g.Min(c => c.StoryId)))
            .OrderBy(g => g.MinStoryId)
            .ToList();

        var resp = new LeaderSelectResponse { LeaderCount = charaGroups.Count };
        if (charaGroups.Count == 0) return resp;

        var allStoryIds = chapters.Select(c => c.StoryId).ToList();
        var progress = await _viewer.GetProgressForChaptersAsync(viewerId, allStoryIds);

        foreach (var (charaId, chaps, _) in charaGroups)
        {
            int highest = 0;
            bool anySkipped = false;
            int clearedCount = 0;
            foreach (var ch in chaps)
            {
                if (progress.TryGetValue(ch.StoryId, out var p) && (p.IsFinish || p.IsSkipped))
                {
                    int row = ChapterRowNum(ch.ChapterId);
                    if (row > highest) highest = row;
                    if (p.IsSkipped) anySkipped = true;
                    clearedCount++;
                }
            }
            resp.LeaderList.Add(new LeaderEntry
            {
                CharaId = charaId,
                IsSkipped = anySkipped,
                IsFinished = clearedCount == chaps.Count,
                CurrentChapter = (highest == 0) ? 1 : highest + 1,
            });
        }

        return resp;
    }
    public async Task<GetDeckListResponse> GetDeckListAsync(StoryApiType apiType, int storyId, long viewerId)
    {
        var byFormat = await _deckRepository.GetDecksByFormats(
            viewerId, new[] { SVSim.Database.Enums.Format.Rotation, SVSim.Database.Enums.Format.Unlimited });

        var resp = new GetDeckListResponse
        {
            UserDeckRotation = byFormat[SVSim.Database.Enums.Format.Rotation]
                .Select(d => new UserDeck(d)).ToList(),
            UserDeckUnlimited = byFormat[SVSim.Database.Enums.Format.Unlimited]
                .Select(d => new UserDeck(d)).ToList(),
            MaintenanceCardList = new List<long>(),
        };

        // The chapter's leader (CharaId == class_id 1-8 for standard classes) drives which
        // prebuilt/trial decks the story deck-select shows. Non-class chapters (custom leaders,
        // chara_id outside 1-8) get empty build/trial lists, matching prod.
        var chapter = await _master.GetChapterByIdAsync(storyId);
        int classId = chapter?.CharaId ?? 0;
        if (classId is >= 1 and <= 8)
        {
            var storyDecks = await _buildDecks.GetStoryDecksByClass(classId);
            resp.BuildDeckList = storyDecks
                .Where(d => d.Kind == StoryDeckKind.Build)
                .Select(ToBuildDeck).ToList();
            resp.TrialDeckList = storyDecks
                .Where(d => d.Kind == StoryDeckKind.Trial)
                .Select(ToTrialDeck).ToList();
        }

        // default_deck_list — all 8 starter decks, keyed by deck_no string (same shape as /deck/info).
        var defaults = await _db.DefaultDecks.OrderBy(d => d.Id).ToListAsync();
        resp.DefaultDeckList = defaults.ToDictionary(
            d => d.Id.ToString(),
            d => new DefaultDeck
            {
                DeckNo = d.DeckNo,
                ClassId = d.ClassId,
                SleeveId = d.SleeveId,
                LeaderSkinId = d.LeaderSkinId,
                DeckName = d.DeckName,
                CardIdArray = JsonSerializer.Deserialize<List<long>>(d.CardIdArray) ?? new(),
                IsCompleteDeck = 1,
                IsAvailableDeck = 1,
                MaintenanceCardIds = new(),
            });

        return resp;
    }

    private static BuildDeck ToBuildDeck(StoryDeckView d) => new()
    {
        DeckNo = d.DeckNo,
        OrderNum = d.OrderNum,
        ClassId = d.ClassId,
        SleeveId = d.SleeveId,
        LeaderSkinId = d.LeaderSkinId,
        EntryNo = d.EntryNo,
        CreateDeckTime = null,
        DeckName = d.DeckName,
        CardIdArray = d.CardIdArray,
        IsCompleteDeck = 1,
        IsAvailableDeck = 1,
        MaintenanceCardIds = new(),
        IsRecommend = d.IsRecommend,
    };

    private static TrialDeck ToTrialDeck(StoryDeckView d) => new()
    {
        DeckNo = d.DeckNo,
        ClassId = d.ClassId,
        SleeveId = d.SleeveId,
        LeaderSkinId = d.LeaderSkinId,
        DeckName = d.DeckName,
        CardIdArray = d.CardIdArray,
        IsCompleteDeck = 1,
        RestrictedCardExists = false,
        IsAvailableDeck = 1,
        MaintenanceCardIds = new(),
        IsIncludeUnPossessionCard = false,
        DeckFormat = d.DeckFormat ?? 0,
        IsRecommend = d.IsRecommend,
    };

    public async Task<StartResponse> StartAsync(StoryApiType apiType, int[] storyIds, long viewerId)
    {
        var resp = new StartResponse();
        for (int i = 0; i < storyIds.Length; i++)
        {
            var chapter = await _master.GetChapterByIdAsync(storyIds[i]);
            if (chapter is null)
            {
                resp[i.ToString()] = Array.Empty<object>();
                continue;
            }
            if (chapter.SpecialBattleSettingId is null)
            {
                resp[i.ToString()] = Array.Empty<object>();
            }
            else
            {
                var sbs = await _master.GetSbsByIdAsync(chapter.SpecialBattleSettingId.Value);
                if (sbs is null) { resp[i.ToString()] = Array.Empty<object>(); continue; }
                resp[i.ToString()] = new StartSlotWithSbs
                {
                    SpecialBattleSetting = new SpecialBattleSettingDto
                    {
                        Id = sbs.Id.ToString(),
                        PlayerFirstTurn = sbs.PlayerFirstTurn.ToString(),
                        PlayerStartPp = sbs.PlayerStartPp.ToString(),
                        EnemyStartPp = sbs.EnemyStartPp.ToString(),
                        PlayerStartLife = sbs.PlayerStartLife.ToString(),
                        EnemyStartLife = sbs.EnemyStartLife.ToString(),
                        PlayerAttachSkill = sbs.PlayerAttachSkill,
                        EnemyAttachSkill = sbs.EnemyAttachSkill,
                        IdOverrideInBattleLog = sbs.IdOverrideInBattleLog,
                        BanishEffectOverride = sbs.BanishEffectOverride,
                        TokenDrawEffectOverride = sbs.TokenDrawEffectOverride,
                        SpecialTokenDrawEffectOverride = sbs.SpecialTokenDrawEffectOverride,
                        ResultSkip = sbs.ResultSkip.ToString(),
                        VsEffectOverride = sbs.VsEffectOverride.ToString(),
                        ClassDestroyEffectOverride = sbs.ClassDestroyEffectOverride.ToString(),
                        Note = sbs.Note ?? "",
                    }
                };
            }
        }
        resp["mission_parameter"] = Array.Empty<object>();
        return resp;
    }
    public async Task<StoryFinishOutcome> FinishAsync(StoryApiType apiType, FinishRequest req, long viewerId)
    {
        var chapter = await _master.GetChapterByIdAsync(req.StoryId);
        if (chapter is null)
        {
            // Sub-chapter story_ids (e.g. section 9 ch.13's vignettes at 375-378) have no chapter
            // master row of their own — they're just progress markers on the parent. The client
            // sends them directly to /finish per StoryFinishTask.GetFinishStoryId. Resolve via the
            // StorySubChapter lookup and record progress at the sub's id with isFinish+isSkipped
            // both true (sub-chapters are always narrative-only — no battle settings on the wire).
            var sub = await _master.FindSubChapterByStoryIdAsync(req.StoryId);
            if (sub is null) return new StoryFinishOutcome(new FinishResponse(), LeveledUp: false, ClassId: null);
            await _viewer.UpsertProgressAsync(viewerId, req.StoryId, isFinish: true, isSkipped: true);
            return new StoryFinishOutcome(new FinishResponse(), LeveledUp: false, ClassId: null);
        }

        var progress = (await _viewer.GetProgressForChaptersAsync(viewerId, new[] { req.StoryId }))
                           .GetValueOrDefault(req.StoryId);

        var resp = new FinishResponse();

        // Three finish shapes:
        //   1. Play-shape (class_id present): user fought the battle → is_finish=true.
        //   2. No-battle chapter + finish=1: narrative-only chapter that the client auto-finishes
        //      with no class_id. Prod marks BOTH is_finish=true AND is_skipped=true — the client
        //      uses is_finish for the green "Cleared" badge, so leaving it false here renders the
        //      blue "AlreadyRead" badge instead (verified against traffic_prod_limited_stories
        //      story_id=1 /info after /finish).
        //   3. Skip-shape on battle chapter: user chose to skip → is_skipped=true only.
        bool isPlayShape = req.IsPlayShape;
        bool isNoBattleAutoFinish = !isPlayShape && !chapter.BattleExists;

        if (isPlayShape || isNoBattleAutoFinish)
        {
            bool firstClear = progress is null || !progress.IsFinish;
            await _viewer.UpsertProgressAsync(
                viewerId, req.StoryId,
                isFinish: true,
                isSkipped: isNoBattleAutoFinish ? true : (bool?)null);

            if (firstClear && chapter.Rewards.Count > 0)
            {
                // Open inventory tx — skip the load entirely when no rewards (narrative-only
                // chapters where the only side effect is the progress upsert).
                await using var tx = await _inv.BeginAsync(viewerId, configure: cfg => cfg.Source = GrantSource.StoryFinish);

                // reward_list and story_reward_list have DIFFERENT semantics for reward_num:
                //   - reward_list:        post-state totals. Client (PlayerStaticData
                //                         .UpdateHaveUserGoodsNum) direct-assigns to in-memory
                //                         balances (e.g. UserRupyCount = num).
                //   - story_reward_list:  deltas. Client (ResultAnimationAgent
                //                         .HandleStoryAndMissionRewards) feeds each entry to
                //                         AddReward(item) which draws a "+N received" popup line.
                // GrantAsync may return 1+N entries (Card grants cascade into cosmetics). All
                // post-state entries go into reward_list via result.RewardList; story_reward_list
                // only gets the top-level mission row's delta (cascade cosmetics have no row).
                var storyRewardDeltas = new List<RewardGrant>();
                foreach (var r in chapter.Rewards)
                {
                    try
                    {
                        await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber);
                    }
                    catch (NotSupportedException ex)
                    {
                        _logger.LogWarning(ex,
                            "StoryService: skipping unsupported reward_type={Type} detail={Detail} num={Num} for story={StoryId}",
                            r.RewardType, r.RewardDetailId, r.RewardNumber, req.StoryId);
                        continue;
                    }
                    // delta for story_reward_list: raw catalog amounts (not post-state)
                    storyRewardDeltas.Add(new RewardGrant
                    {
                        RewardType = ((int)r.RewardType).ToString(),
                        RewardId = r.RewardDetailId.ToString(),
                        RewardNum = r.RewardNumber.ToString(),
                    });
                }

                var result = await tx.CommitAsync();

                // reward_list = post-state totals from tx (includes cosmetic cascade entries)
                foreach (var g in result.RewardList)
                {
                    resp.RewardList.Add(new RewardGrant
                    {
                        RewardType = ((int)g.RewardType).ToString(),
                        RewardId = g.RewardId.ToString(),
                        RewardNum = g.RewardNum.ToString(),
                    });
                }
                // story_reward_list = deltas accumulated above
                resp.StoryRewardList.AddRange(storyRewardDeltas);
            }

            bool leveledUp = false;
            if (firstClear && isPlayShape)
            {
                // XP grant requires a class_id (only sent on play-shape). No-battle chapters
                // have no class context — prod returns get_class_experience=0 for them.
                var xpViewer = await _viewers.LoadForBattleXpGrantAsync(viewerId);
                if (xpViewer is not null && req.ClassId.HasValue)
                {
                    var xp = await _xp.GrantAsync(xpViewer, req.ClassId.Value, isWin: true, BattleXpMode.Story);
                    await _db.SaveChangesAsync();
                    resp.GetClassExperience = xp.GetXp.ToString();
                    resp.ClassExperience = xp.TotalXp;
                    resp.ClassLevel = xp.Level.ToString();
                    leveledUp = xp.LeveledUp;
                }
            }

            return new StoryFinishOutcome(resp, LeveledUp: leveledUp, ClassId: req.ClassId);
        }
        else
        {
            // Skip-shape: optionally unlock a branch child if selection_chapter_id is set.
            if (!string.IsNullOrEmpty(req.SelectionChapterId))
            {
                var siblings = await _master.GetChaptersBySectionCharaAsync(chapter.SectionId, chapter.CharaId);
                var child = siblings.FirstOrDefault(c => c.ChapterId == req.SelectionChapterId);
                if (child is not null)
                    await _viewer.UpsertBranchUnlockAsync(viewerId, child.StoryId);
            }
            await _viewer.UpsertProgressAsync(viewerId, req.StoryId, isFinish: null, isSkipped: true);
        }

        return new StoryFinishOutcome(resp, LeveledUp: false, ClassId: null);
    }
    public async Task<FinishResponse> AllFinishAsync(StoryApiType apiType, int[] storyIds, bool isFinish, long viewerId)
    {
        foreach (var sid in storyIds)
            await _viewer.UpsertProgressAsync(viewerId, sid, isFinish: null, isSkipped: true);
        return new FinishResponse();
    }

    private static int ChapterRowNum(string chapterId)
    {
        // Extract leading numeric prefix; for "12a" returns 12.
        int i = 0;
        while (i < chapterId.Length && char.IsDigit(chapterId[i])) i++;
        return int.TryParse(chapterId[..i], out int n) ? n : 0;
    }
}
