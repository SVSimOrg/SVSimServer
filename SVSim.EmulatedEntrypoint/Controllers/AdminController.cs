using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SVSim.Database;
using SVSim.Database.Entities.Story;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Infrastructure;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Admin;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Admin;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Util endpoints for bootstrapping the dev environment. Actions are gated by
/// <see cref="RequireAdminSecretAttribute"/>: callers must send the shared secret from
/// <c>Admin:ImportSecret</c> in the <c>X-Admin-Secret</c> header. Missing/blank config
/// disables the endpoint (fail-closed).
/// </summary>
public class AdminController : SVSimController
{
    private readonly IViewerRepository _viewerRepository;
    private readonly SVSimDbContext _dbContext;
    private readonly ILogger<AdminController> _logger;
    private readonly IGameCalendarService _calendar;

    public AdminController(IViewerRepository viewerRepository, SVSimDbContext dbContext,
        ILogger<AdminController> logger, IGameCalendarService calendar)
    {
        _viewerRepository = viewerRepository;
        _dbContext = dbContext;
        _logger = logger;
        _calendar = calendar;
    }

    /// <summary>
    /// Upsert a viewer from external data (typically captured from the live game via the
    /// SVSimLoader dump). Matches existing viewers by SteamId; creates a new one if missing.
    /// Only essential fields are imported today — extend as needed.
    /// </summary>
    [AllowAnonymous]
    [RequireAdminSecret]
    [HttpPost("import_viewer")]
    public async Task<ActionResult<ImportViewerResponse>> ImportViewer(ImportViewerRequest request)
    {
        if (request.SteamId == 0)
        {
            return BadRequest("steam_id is required");
        }

        // SocialAccountConnection is [Owned]-by-Viewer — can't query the owned table directly;
        // look up the Viewer with a matching owned connection instead.
        var existing = await _dbContext.Viewers
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.SocialAccountConnections.Any(sac =>
                sac.AccountType == SocialAccountType.Steam && sac.AccountId == request.SteamId));

        long viewerId;
        bool wasCreated;

        if (existing is null)
        {
            var created = await _viewerRepository.RegisterViewer(
                request.DisplayName ?? "Imported Viewer",
                SocialAccountType.Steam,
                request.SteamId);
            viewerId = created.Id;
            wasCreated = true;
        }
        else
        {
            viewerId = existing.Id;
            wasCreated = false;
        }

        // Reload with all the nav properties we need to mutate. RegisterViewer SaveChanges'd
        // already, so we re-fetch with full graph and apply the updates. AsSplitQuery to avoid
        // the cartesian-explosion across all the many-to-many cosmetic collections.
        var viewer = await _dbContext.Viewers
            .AsSplitQuery()
            .Include(v => v.Info).ThenInclude(i => i.SelectedEmblem)
            .Include(v => v.Info).ThenInclude(i => i.SelectedDegree)
            .Include(v => v.Currency)
            .Include(v => v.MissionData)
            .Include(v => v.Missions)
            .Include(v => v.Achievements)
            .Include(v => v.Classes).ThenInclude(c => c.Class)
            .Include(v => v.Sleeves)
            .Include(v => v.Emblems)
            .Include(v => v.Degrees)
            .Include(v => v.LeaderSkins)
            .Include(v => v.MyPageBackgrounds)
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .Include(v => v.Decks).ThenInclude(d => d.Cards)
            .FirstAsync(v => v.Id == viewerId);

        if (request.DisplayName is not null) viewer.DisplayName = request.DisplayName;
        if (request.CountryCode is not null) viewer.Info.CountryCode = request.CountryCode;
        if (request.TutorialState.HasValue) viewer.MissionData.TutorialState = request.TutorialState.Value;

        if (request.Currency is not null)
        {
            if (request.Currency.Crystals.HasValue) viewer.Currency.Crystals = request.Currency.Crystals.Value;
            if (request.Currency.Rupees.HasValue) viewer.Currency.Rupees = request.Currency.Rupees.Value;
            if (request.Currency.RedEther.HasValue) viewer.Currency.RedEther = request.Currency.RedEther.Value;
        }

        if (request.SelectedEmblemId.HasValue)
        {
            var emblem = await _dbContext.Emblems.FindAsync(request.SelectedEmblemId.Value);
            if (emblem is not null) viewer.Info.SelectedEmblem = emblem;
        }
        if (request.SelectedDegreeId.HasValue)
        {
            var degree = await _dbContext.Degrees.FindAsync(request.SelectedDegreeId.Value);
            if (degree is not null) viewer.Info.SelectedDegree = degree;
        }

        await ReplaceOwned(viewer.Sleeves, request.OwnedSleeveIds, _dbContext.Sleeves);
        await ReplaceOwned(viewer.Emblems, request.OwnedEmblemIds, _dbContext.Emblems);
        await ReplaceOwned(viewer.Degrees, request.OwnedDegreeIds, _dbContext.Degrees);
        await ReplaceOwned(viewer.LeaderSkins, request.OwnedLeaderSkinIds, _dbContext.LeaderSkins);
        await ReplaceOwned(viewer.MyPageBackgrounds, request.OwnedMyPageBackgroundIds, _dbContext.MyPageBackgrounds);

        if (request.Classes is not null)
        {
            foreach (var importClass in request.Classes)
            {
                var existingClass = viewer.Classes.FirstOrDefault(c => c.Class.Id == importClass.ClassId);
                if (existingClass is not null)
                {
                    existingClass.Level = importClass.Level;
                    existingClass.Exp = importClass.Exp;
                }
            }
        }

        // Accumulates distinct card_ids referenced by the import (owned list + deck lists)
        // that aren't in our card master. Surfaced in the response and logged after save.
        var skippedCardIds = new HashSet<long>();

        if (request.OwnedCards is not null)
        {
            var wanted = request.OwnedCards
                .GroupBy(c => c.CardId)
                .Select(g => g.First())
                .ToList();
            var ids = wanted.Select(c => c.CardId).ToList();
            var cardMaster = await _dbContext.Cards
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);

            viewer.Cards.Clear();
            foreach (var c in wanted)
            {
                if (!cardMaster.TryGetValue(c.CardId, out var card))
                {
                    skippedCardIds.Add(c.CardId);
                    continue;
                }
                viewer.Cards.Add(new OwnedCardEntry
                {
                    Card = card,
                    Count = Math.Clamp(c.Count, 1, OwnedCardEntry.MaxCopies),
                    IsProtected = c.IsProtected,
                });
            }
        }

        if (request.Items is not null)
        {
            var wanted = request.Items
                .GroupBy(i => i.ItemId)
                .Select(g => g.First())
                .ToList();
            var ids = wanted.Select(i => i.ItemId).ToList();
            var itemMaster = await _dbContext.Items
                .Where(i => ids.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id);

            viewer.Items.Clear();
            foreach (var i in wanted)
            {
                if (!itemMaster.TryGetValue(i.ItemId, out var item)) continue; // unknown master id
                viewer.Items.Add(new OwnedItemEntry { Item = item, Count = i.Count, Viewer = viewer });
            }
        }

        if (request.Decks is not null)
        {
            var allDeckCardIds = request.Decks
                .Where(d => d.CardIdArray is not null)
                .SelectMany(d => d.CardIdArray!)
                .Distinct()
                .ToList();
            var deckCardMaster = await _dbContext.Cards
                .Where(c => allDeckCardIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);
            var classes = await _dbContext.Classes.Include(c => c.LeaderSkins).ToDictionaryAsync(c => c.Id);
            var sleeves = await _dbContext.Sleeves.ToDictionaryAsync(s => (long)s.Id);
            var leaderSkins = await _dbContext.LeaderSkins.ToDictionaryAsync(s => s.Id);
            var defaultSleeve = await _dbContext.Sleeves.FindAsync((int)DefaultSleeveId);
            var latestMyRotationId = (await _dbContext.MyRotationSettings.AsNoTracking()
                .Select(s => (int?)s.Id)
                .OrderByDescending(id => id)
                .FirstOrDefaultAsync())?.ToString();

            _dbContext.RemoveRange(viewer.Decks);
            viewer.Decks.Clear();

            foreach (var d in request.Decks)
            {
                // A /load/index dump carries every deck slot, most of them empty placeholders
                // (no cards). Skip them: the client manages empty slots itself (it's why the old
                // default-deck cloning was removed), and importing empty MyRotation slots would
                // otherwise persist decks with a bogus rotation id.
                if ((d.CardIdArray?.Count ?? 0) == 0) continue;

                Format format;
                try { format = FormatExtensions.FromApi(d.DeckFormat); }
                catch (ArgumentOutOfRangeException) { continue; } // skip unsupported wire format
                if (!classes.TryGetValue(d.ClassId, out var classEntry)) continue;

                SleeveEntry? sleeve = null;
                if (d.SleeveId.HasValue) sleeves.TryGetValue(d.SleeveId.Value, out sleeve);
                sleeve ??= defaultSleeve;

                LeaderSkinEntry? leaderSkin = null;
                if (d.LeaderSkinId.HasValue) leaderSkins.TryGetValue(d.LeaderSkinId.Value, out leaderSkin);
                leaderSkin ??= classEntry.DefaultLeaderSkin ?? classEntry.LeaderSkins.FirstOrDefault();

                if (sleeve is null || leaderSkin is null) continue;

                var cards = (d.CardIdArray ?? new List<long>())
                    .GroupBy(id => id)
                    .Where(g =>
                    {
                        if (deckCardMaster.ContainsKey(g.Key)) return true;
                        skippedCardIds.Add(g.Key);
                        return false;
                    })
                    .Select(g => new DeckCard { Card = deckCardMaster[g.Key], Count = g.Count() })
                    .ToList();

                viewer.Decks.Add(new ShadowverseDeckEntry
                {
                    Name = d.DeckName ?? $"Deck {d.DeckNo}",
                    Number = d.DeckNo,
                    Format = format,
                    Class = classEntry,
                    Sleeve = sleeve,
                    LeaderSkin = leaderSkin,
                    RandomLeaderSkin = (d.IsRandomLeaderSkin ?? 0) != 0,
                    Cards = cards,
                    MyRotationId = format == Format.MyRotation ? (d.MyRotationId ?? latestMyRotationId) : null,
                });
            }
        }

        // Pass A: MissionMeta scalars
        if (request.MissionMeta is { } meta)
        {
            viewer.MissionData ??= new ViewerMissionData();
            if (meta.HasReceivedPickTwoMission is { } hrptm)
                viewer.MissionData.HasReceivedPickTwoMission = hrptm != 0;
            if (meta.MissionReceiveType is { } mrt)
                viewer.MissionData.MissionReceiveType = mrt;
            if (meta.MissionChangeTime is { } mct
                && DateTime.TryParseExact(mct, "yyyy-MM-dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var mctParsed))
            {
                viewer.MissionData.MissionChangeTime = mctParsed;
            }
        }

        // Pass B: Missions + ViewerEventCounter
        var skippedMissionIds = new HashSet<int>();
        var skippedMissionCounterIds = new HashSet<int>();
        if (request.Missions is { } missions)
        {
            // Snapshot the OLD mission catalog ids BEFORE clearing, then load their catalog
            // entries so we can compute which (EventKey, Period) counter keys are going away.
            var oldMissionCatalogIds = viewer.Missions.Select(m => m.MissionCatalogId).Distinct().ToList();
            var nowUtcForOld = DateTimeOffset.UtcNow;
            var oldCatalogs = await _dbContext.MissionCatalog
                .Where(c => oldMissionCatalogIds.Contains(c.Id))
                .ToListAsync();
            var oldMissionCounterKeys = oldCatalogs
                .Select(c => ResolveMissionCounter(c, nowUtcForOld))
                .Where(r => r is not null)
                .Select(r => r!.Value)
                .ToHashSet();

            viewer.Missions.Clear();
            var missionIds = missions.Select(m => m.MissionId).Distinct().ToList();
            var catalogs = await _dbContext.MissionCatalog
                .Where(c => missionIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);
            var nowUtc = DateTimeOffset.UtcNow;

            // Resolve counter keys for all missions up-front so we can load existing counters
            // in one query and avoid duplicate-Add races when two missions share the same
            // (EventKey, Period) within the same import batch.
            var resolvedCounterKeys = new Dictionary<int, (string EventKey, string Period)>();
            foreach (var m in missions)
            {
                if (!catalogs.TryGetValue(m.MissionId, out var cat)) continue;
                var r = ResolveMissionCounter(cat, nowUtc);
                if (r is not null)
                    resolvedCounterKeys[m.MissionId] = r.Value;
            }

            // Delete orphan ViewerEventCounter rows: keys that existed for OLD missions but are
            // NOT referenced by any mission in the new payload.
            var newMissionCounterKeys = resolvedCounterKeys.Values.ToHashSet();
            var orphanMissionKeys = oldMissionCounterKeys
                .Where(k => !newMissionCounterKeys.Contains(k))
                .ToList();
            if (orphanMissionKeys.Count > 0)
            {
                var orphanEventKeys = orphanMissionKeys.Select(k => k.EventKey).Distinct().ToList();
                var orphanPeriods   = orphanMissionKeys.Select(k => k.Period).Distinct().ToList();
                var orphanCounters = await _dbContext.ViewerEventCounters
                    .Where(c => c.ViewerId == viewer.Id
                             && orphanEventKeys.Contains(c.EventKey)
                             && orphanPeriods.Contains(c.Period))
                    .ToListAsync();
                // Filter in-memory to the exact (EventKey, Period) pairs (the DB query widens slightly).
                var toDelete = orphanCounters
                    .Where(c => orphanMissionKeys.Contains((c.EventKey, c.Period)))
                    .ToList();
                _dbContext.ViewerEventCounters.RemoveRange(toDelete);
            }

            // Collect all distinct (EventKey, Period) pairs we will touch.
            var allEventKeys = resolvedCounterKeys.Values.Select(v => v.EventKey).Distinct().ToList();
            var allPeriods   = resolvedCounterKeys.Values.Select(v => v.Period).Distinct().ToList();

            // Load existing counters in one query (widens slightly; filter in memory below — fine for small N).
            var existingCounters = await _dbContext.ViewerEventCounters
                .Where(c => c.ViewerId == viewer.Id
                         && allEventKeys.Contains(c.EventKey)
                         && allPeriods.Contains(c.Period))
                .ToListAsync();
            var counterCache = existingCounters
                .ToDictionary(c => (c.EventKey, c.Period));

            foreach (var m in missions)
            {
                if (!catalogs.TryGetValue(m.MissionId, out var cat))
                {
                    skippedMissionIds.Add(m.MissionId);
                    continue;
                }
                viewer.Missions.Add(new ViewerMission
                {
                    MissionCatalogId = cat.Id,
                    Slot = m.Slot ?? (cat.LotType == 6 ? 0 : 1),
                    MissionStatus = m.MissionStatus,
                    AssignedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ClaimedAt = null
                });

                if (!resolvedCounterKeys.TryGetValue(m.MissionId, out var resolved))
                {
                    skippedMissionCounterIds.Add(m.MissionId);
                    continue;
                }
                var (eventKey, period) = resolved;
                if (!counterCache.TryGetValue((eventKey, period), out var counter))
                {
                    counter = new ViewerEventCounter
                    {
                        ViewerId = viewer.Id,
                        EventKey = eventKey,
                        Period = period,
                    };
                    _dbContext.ViewerEventCounters.Add(counter);
                    counterCache[(eventKey, period)] = counter;
                }
                counter.Count = m.TotalCount;
            }
        }

        // Pass C: Achievements + ViewerEventCounter
        var skippedAchievementTypes = new HashSet<int>();
        var skippedAchievementCounterTypes = new HashSet<int>();
        if (request.Achievements is { } achievements)
        {
            // Snapshot the OLD achievement types BEFORE clearing, then load their catalog entries
            // so we can compute which (EventKey, Period) counter keys are going away.
            var oldAchievementTypes = viewer.Achievements.Select(a => a.AchievementType).Distinct().ToList();
            var oldAchCatalogs = await _dbContext.AchievementCatalog
                .Where(c => oldAchievementTypes.Contains(c.AchievementType))
                .GroupBy(c => c.AchievementType)
                .ToDictionaryAsync(g => g.Key, g => g.First());
            var oldAchievementCounterKeys = oldAchCatalogs.Values
                .Select(c => ResolveAchievementCounter(c))
                .Where(r => r is not null)
                .Select(r => r!.Value)
                .ToHashSet();

            viewer.Achievements.Clear();
            var types = achievements.Select(a => a.AchievementType).Distinct().ToList();
            var catalogs = await _dbContext.AchievementCatalog
                .Where(c => types.Contains(c.AchievementType))
                .GroupBy(c => c.AchievementType)
                .ToDictionaryAsync(g => g.Key, g => g.First());

            // Resolve counter keys for all achievements up-front so we can load existing counters
            // in one query and avoid duplicate-Add races when two achievements share the same
            // EventType (same (EventKey, AllTime) cache key) within the same import batch.
            var resolvedAchievementCounterKeys = new Dictionary<int, (string EventKey, string Period)>();
            foreach (var a in achievements)
            {
                if (!catalogs.TryGetValue(a.AchievementType, out var cat)) continue;
                var r = ResolveAchievementCounter(cat);
                if (r is not null)
                    resolvedAchievementCounterKeys[a.AchievementType] = r.Value;
            }

            // Delete orphan ViewerEventCounter rows: keys that existed for OLD achievements but
            // are NOT referenced by any achievement in the new payload.
            var newAchievementCounterKeys = resolvedAchievementCounterKeys.Values.ToHashSet();
            var orphanAchKeys = oldAchievementCounterKeys
                .Where(k => !newAchievementCounterKeys.Contains(k))
                .ToList();
            if (orphanAchKeys.Count > 0)
            {
                var orphanAchEventKeys = orphanAchKeys.Select(k => k.EventKey).Distinct().ToList();
                var orphanAchPeriods   = orphanAchKeys.Select(k => k.Period).Distinct().ToList();
                var orphanAchCounters = await _dbContext.ViewerEventCounters
                    .Where(c => c.ViewerId == viewer.Id
                             && orphanAchEventKeys.Contains(c.EventKey)
                             && orphanAchPeriods.Contains(c.Period))
                    .ToListAsync();
                // Filter in-memory to the exact (EventKey, Period) pairs.
                var toDeleteAch = orphanAchCounters
                    .Where(c => orphanAchKeys.Contains((c.EventKey, c.Period)))
                    .ToList();
                _dbContext.ViewerEventCounters.RemoveRange(toDeleteAch);
            }

            // Collect all distinct (EventKey, Period) pairs we will touch.
            var allAchEventKeys = resolvedAchievementCounterKeys.Values.Select(v => v.EventKey).Distinct().ToList();
            var allAchPeriods   = resolvedAchievementCounterKeys.Values.Select(v => v.Period).Distinct().ToList();

            // Load existing counters in one query.
            var existingAchCounters = await _dbContext.ViewerEventCounters
                .Where(c => c.ViewerId == viewer.Id
                         && allAchEventKeys.Contains(c.EventKey)
                         && allAchPeriods.Contains(c.Period))
                .ToListAsync();
            var achievementCounterCache = existingAchCounters
                .ToDictionary(c => (c.EventKey, c.Period));

            foreach (var a in achievements)
            {
                if (!catalogs.TryGetValue(a.AchievementType, out var cat))
                {
                    skippedAchievementTypes.Add(a.AchievementType);
                    continue;
                }
                viewer.Achievements.Add(new ViewerAchievement
                {
                    ViewerId = viewer.Id,
                    AchievementType = a.AchievementType,
                    Level = a.Level,
                    NowAchievedLevel = a.NowAchievedLevel,
                    ResultAnnounceSawLevel = a.ResultAnnounceSawLevel,
                    AchievementStatus = 0
                });

                if (!resolvedAchievementCounterKeys.TryGetValue(a.AchievementType, out var resolved))
                {
                    skippedAchievementCounterTypes.Add(a.AchievementType);
                    continue;
                }
                var (eventKey, period) = resolved;
                if (!achievementCounterCache.TryGetValue((eventKey, period), out var counter))
                {
                    counter = new ViewerEventCounter
                    {
                        ViewerId = viewer.Id,
                        EventKey = eventKey,
                        Period = period,
                    };
                    _dbContext.ViewerEventCounters.Add(counter);
                    achievementCounterCache[(eventKey, period)] = counter;
                }
                counter.Count = a.TotalCount;
            }
        }

        // Pass D: StoryProgress with per-family offsets
        var skippedStoryIds = new HashSet<long>();
        if (request.StoryProgress is { } storyRows)
        {
            var existingStory = await _dbContext.ViewerStoryProgress
                .Where(p => p.ViewerId == viewer.Id).ToListAsync();
            _dbContext.ViewerStoryProgress.RemoveRange(existingStory);

            // Compute effective ids and bulk-load referenced catalog rows.
            var effectiveIds = new List<int>(storyRows.Count);
            var rowEffective = new List<(ImportStoryProgress row, int effective)>(storyRows.Count);
            foreach (var r in storyRows)
            {
                long offset = r.StoryApiType switch
                {
                    1 => 0L,
                    2 => 10_000_000L,
                    3 => 20_000_000L,
                    _ => -1L
                };
                if (offset < 0) { skippedStoryIds.Add(r.StoryId); continue; }
                int wireId = r.SubChapterId ?? r.StoryId;
                int effective = checked((int)(wireId + offset));
                effectiveIds.Add(effective);
                rowEffective.Add((r, effective));
            }
            var present = (await _dbContext.StoryWorlds
                .Where(s => effectiveIds.Contains(s.Id))
                .Select(s => s.Id).ToListAsync()).ToHashSet();

            var nowUtc = DateTime.UtcNow;
            foreach (var (r, eff) in rowEffective)
            {
                if (!present.Contains(eff)) { skippedStoryIds.Add(eff); continue; }
                _dbContext.ViewerStoryProgress.Add(new ViewerStoryProgress
                {
                    ViewerId = viewer.Id,
                    StoryId = eff,
                    IsFinish = r.IsFinish,
                    IsSkipped = r.IsSkipped,
                    FinishedAt = r.IsFinish ? nowUtc : null,
                    SkippedAt  = r.IsSkipped ? nowUtc : null
                });
            }
        }

        await _dbContext.SaveChangesAsync();

        if (skippedCardIds.Count > 0)
        {
            _logger.LogWarning(
                "ImportViewer (steam_id={SteamId}, viewer_id={ViewerId}): skipped {Count} unknown " +
                "card_id(s) not present in the card master. Sample: [{Sample}]",
                request.SteamId, viewer.Id, skippedCardIds.Count,
                string.Join(", ", skippedCardIds.Take(20)));
        }
        if (skippedMissionIds.Count > 0)
        {
            _logger.LogWarning(
                "ImportViewer (steam_id={SteamId}, viewer_id={ViewerId}): skipped {Count} unknown mission_id(s). Sample: [{Sample}]",
                request.SteamId, viewer.Id, skippedMissionIds.Count,
                string.Join(", ", skippedMissionIds.Take(10)));
        }
        if (skippedMissionCounterIds.Count > 0)
        {
            _logger.LogWarning(
                "ImportViewer (steam_id={SteamId}, viewer_id={ViewerId}): {Count} mission(s) had unresolvable EventKey, counter not written. Sample: [{Sample}]",
                request.SteamId, viewer.Id, skippedMissionCounterIds.Count,
                string.Join(", ", skippedMissionCounterIds.Take(10)));
        }
        if (skippedAchievementTypes.Count > 0)
        {
            _logger.LogWarning(
                "ImportViewer (steam_id={SteamId}, viewer_id={ViewerId}): skipped {Count} unknown achievement_type(s). Sample: [{Sample}]",
                request.SteamId, viewer.Id, skippedAchievementTypes.Count,
                string.Join(", ", skippedAchievementTypes.Take(10)));
        }
        if (skippedAchievementCounterTypes.Count > 0)
        {
            _logger.LogWarning(
                "ImportViewer (steam_id={SteamId}, viewer_id={ViewerId}): {Count} achievement(s) had unresolvable EventKey, counter not written. Sample: [{Sample}]",
                request.SteamId, viewer.Id, skippedAchievementCounterTypes.Count,
                string.Join(", ", skippedAchievementCounterTypes.Take(10)));
        }
        if (skippedStoryIds.Count > 0)
        {
            _logger.LogWarning(
                "ImportViewer (steam_id={SteamId}, viewer_id={ViewerId}): skipped {Count} unknown story_id(s). Sample: [{Sample}]",
                request.SteamId, viewer.Id, skippedStoryIds.Count,
                string.Join(", ", skippedStoryIds.Take(10)));
        }

        return new ImportViewerResponse
        {
            ViewerId = viewer.Id,
            ShortUdid = viewer.ShortUdid,
            WasCreated = wasCreated,
            SkippedCardCount = skippedCardIds.Count,
            SkippedMissionCount = skippedMissionIds.Count,
            SkippedMissionCounterCount = skippedMissionCounterIds.Count,
            SkippedAchievementCount = skippedAchievementTypes.Count,
            SkippedAchievementCounterCount = skippedAchievementCounterTypes.Count,
            SkippedStoryCount = skippedStoryIds.Count,
        };
    }

    /// <summary>
    /// Replaces the owned-collection with the master rows matching the supplied ids.
    /// Null `ids` is a no-op (preserve existing). Empty list clears the collection.
    /// </summary>
    private async Task ReplaceOwned<TEntity>(List<TEntity> owned, List<int>? ids, DbSet<TEntity> table)
        where TEntity : class
    {
        if (ids is null) return;
        owned.Clear();
        if (ids.Count == 0) return;

        var rows = await table.Where(e => ids.Contains(EF.Property<int>(e, "Id"))).ToListAsync();
        owned.AddRange(rows);
    }

    // TODO: unify with MissionAssembler.cs — same logic duplicated here.
    private (string EventKey, string Period)? ResolveMissionCounter(MissionCatalogEntry catalog, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrEmpty(catalog.EventType)) return null;
        var period = catalog.LotType switch
        {
            6 => _calendar.DayKey(nowUtc),
            2 => _calendar.WeekKey(nowUtc),
            _ => null
        };
        if (period is null) return null;
        return (catalog.EventType!, period);
    }

    // TODO: unify with MissionAssembler.cs — same logic duplicated here.
    private static (string EventKey, string Period)? ResolveAchievementCounter(AchievementCatalogEntry catalog)
    {
        if (string.IsNullOrEmpty(catalog.EventType)) return null;
        return (catalog.EventType!, GameCalendarPeriods.AllTime);
    }

    /// <summary>
    /// Fallback sleeve id used when an imported deck has no resolvable <c>sleeve_id</c>.
    /// 3000011 is prod's default deck sleeve.
    /// </summary>
    private const long DefaultSleeveId = 3000011L;
}
