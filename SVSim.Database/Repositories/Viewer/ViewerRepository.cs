using Microsoft.EntityFrameworkCore;
using Npgsql;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;

namespace SVSim.Database.Repositories.Viewer;

public class ViewerRepository : IViewerRepository
{
    protected readonly SVSimDbContext _dbContext;
    private readonly IGameConfigService _config;

    private const int MaxFriends = 20;

    public ViewerRepository(SVSimDbContext dbContext, IGameConfigService config)
    {
        _dbContext = dbContext;
        _config = config;
    }

    public async Task<Models.Viewer?> GetViewerBySocialConnection(SocialAccountType accountType, ulong socialId)
    {
        // SocialAccountConnection is [Owned]-by-Viewer — can't be queried as a top-level Set<T>.
        // Look up the Viewer that has a matching owned connection instead.
        return await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.SocialAccountConnections.Any(sac =>
                sac.AccountType == accountType && sac.AccountId == socialId));
    }

    public async Task<Models.Viewer?> GetViewerWithSocials(long id)
    {
        return await _dbContext.Set<Models.Viewer>().AsNoTracking().Include(viewer => viewer.SocialAccountConnections)
            .FirstOrDefaultAsync(viewer => viewer.Id == id);
    }

    /// <summary>
    /// Loads a viewer with every navigation property needed to render the home-screen
    /// (/load/index). Heavy query — only call from LoadController.Index.
    /// </summary>
    public async Task<Models.Viewer?> GetViewerByShortUdid(long shortUdid)
    {
        // AsSplitQuery: each Include() collection runs as a separate SELECT instead of one giant
        // LEFT JOIN with a cartesian product on the result set. The combined Decks+DeckCard+Cards+
        // many-to-many-cosmetics shape was producing hundreds of thousands of duplicate rows after
        // the import-time default-deck clone landed (24 decks × 40 DeckCards × N cosmetics each),
        // pushing /load/index to ~17 s/request. Split queries take O(rows) total instead.
        return await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.MissionData)
            .Include(v => v.Info).ThenInclude(i => i.SelectedEmblem)
            .Include(v => v.Info).ThenInclude(i => i.SelectedDegree)
            .Include(v => v.Currency)
            .Include(v => v.Classes).ThenInclude(c => c.Class).ThenInclude(c => c.LeaderSkins)
            .Include(v => v.Classes).ThenInclude(c => c.LeaderSkin)
            .Include(v => v.Decks).ThenInclude(d => d.Class)
            .Include(v => v.Decks).ThenInclude(d => d.Sleeve)
            .Include(v => v.Decks).ThenInclude(d => d.LeaderSkin)
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .Include(v => v.Sleeves)
            .Include(v => v.Emblems)
            .Include(v => v.Degrees)
            .Include(v => v.LeaderSkins).ThenInclude(ls => ls.Class)
            .Include(v => v.MyPageBackgrounds)
            .Include(v => v.MyPageBgRotation)
            .FirstOrDefaultAsync(viewer => viewer.ShortUdid == shortUdid);
    }

    public async Task<Models.Viewer> RegisterViewer(string displayName, SocialAccountType socialType,
        ulong socialAccountIdentifier, ulong? shortUdid = null)
    {
        // RegisterViewer is the import / Steam-social path. Default to the post-tutorial baseline
        // (state 100) so AdminController.ImportViewer materializes prod-replicas at the home screen
        // unless the import request explicitly overrides via request.TutorialState. The anonymous
        // signup path (RegisterAnonymousViewer) uses the parameter default of 1.
        var viewer = await BuildDefaultViewer(displayName, initialTutorialState: 100);
        viewer.SocialAccountConnections.Add(new SocialAccountConnection
        {
            AccountId = socialAccountIdentifier,
            AccountType = socialType
        });
        _dbContext.Set<Models.Viewer>().Add(viewer);
        await _dbContext.SaveChangesAsync();
        return viewer;
    }

    public async Task<Models.Viewer?> GetViewerByUdid(Guid udid)
    {
        if (udid == Guid.Empty) return null;
        return await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Udid == udid);
    }

    public async Task<Models.Viewer> RegisterAnonymousViewer(Guid udid)
    {
        if (udid == Guid.Empty)
            throw new InvalidOperationException("Cannot register viewer for empty UDID.");

        // Empty DisplayName is load-bearing: the client's Wizard.Title/UserNameInput.Start
        // does `IsFinished = !string.IsNullOrEmpty(PlayerStaticData.UserName);` — IsFinished
        // true skips the dialog AND the /tutorial/update_action #1 + /account/update_name
        // calls that accompany it. Any non-empty value (including the " - " placeholder this
        // method used to pass) trips that check and silently bypasses the name-entry sub-step.
        // Empty string flows through /load/index → user_info.name → PlayerStaticData.UserName,
        // and the title screen surfaces the input dialog.
        //
        // SkipTutorialConfig.Enabled: dev-mode fast path. When true, fresh signups land at
        // state 100 (post-tutorial baseline) instead of TUTORIAL_STEP0 — cuts the walk-through-
        // tutorial time after every wiped identity in the two-client PVP smoke. Off by default.
        int initialTutorialState = _config.Get<SkipTutorialConfig>().Enabled ? 100 : 1;
        var viewer = await BuildDefaultViewer("", initialTutorialState: initialTutorialState);
        viewer.Udid = udid;
        _dbContext.Set<Models.Viewer>().Add(viewer);

        // Eager-seed the tutorial gifts so the inbox is populated by the time the tutorial
        // walks the user to it (which happens AFTER initial battles, per the gift-inbox
        // design). The catalogue lives in TutorialPresentEntries (loaded from
        // SVSim.Bootstrap/Data/seeds/tutorial-presents.json); if the catalogue is empty
        // (bootstrap not run) signup still succeeds with an empty inbox. The unique
        // (ViewerId, PresentId) index is the backstop against double-seeding on a retried
        // signup. Both inserts commit in a single SaveChanges.
        var tutorialPresents = await _dbContext.Set<TutorialPresentEntry>()
            .AsNoTracking()
            .OrderBy(p => p.PresentId)
            .ToListAsync();
        var createdAt = DateTime.UtcNow;
        foreach (var spec in tutorialPresents)
        {
            _dbContext.Set<ViewerPresent>().Add(new ViewerPresent
            {
                Viewer         = viewer,        // EF wires up ViewerId via the nav after Insert
                PresentId      = spec.PresentId,
                Status         = PresentStatus.Unclaimed,
                RewardType     = spec.RewardType,
                RewardDetailId = spec.RewardDetailId,
                RewardCount    = spec.RewardCount,
                ItemType       = spec.ItemType,
                Message        = spec.Message,
                CreatedAt      = createdAt,
                Source         = "tutorial",
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Concurrent signup for the same UDID raced us to the unique index. The other request
            // already committed a viewer with this UDID — re-read and return it. Detach the local
            // entity first so EF doesn't keep trying to insert the now-orphaned graph.
            //
            // Cross-engine: Postgres surfaces this as Npgsql.PostgresException SqlState "23505";
            // SQLite (test backend) surfaces it as Microsoft.Data.Sqlite.SqliteException with
            // SqliteErrorCode 19 (SQLITE_CONSTRAINT). Matched by type-name to avoid pulling a
            // Sqlite package dep into SVSim.Database.
            _dbContext.Entry(viewer).State = EntityState.Detached;
            var existing = await GetViewerByUdid(udid);
            if (existing is not null) return existing;

            // Lookup-by-UDID missed → the violation wasn't on the UDID index. Pull the constraint
            // name out of the inner exception so the caller can see which constraint actually
            // blocked the insert (Steam social uniqueness, owned-collection uniqueness, etc.).
            string constraintName = ExtractConstraintName(ex);
            throw new InvalidOperationException(
                $"Got unique-violation on viewer insert for Udid={udid} but the UDID is not in the table. " +
                $"The violated constraint was '{constraintName}'. " +
                "Original exception preserved as InnerException.",
                ex);
        }
        return viewer;
    }

    /// <summary>
    /// Extracts the violated constraint name from a wrapped backend exception, when available.
    /// Postgres surfaces this as <c>PostgresException.ConstraintName</c>. Returns "&lt;unknown&gt;"
    /// for other backends or when the name can't be reflected out.
    /// </summary>
    private static string ExtractConstraintName(DbUpdateException ex)
    {
        if (ex.InnerException is Npgsql.PostgresException pgEx && !string.IsNullOrEmpty(pgEx.ConstraintName))
        {
            return pgEx.ConstraintName;
        }
        // SQLite doesn't expose a constraint name in a structured field — fall back to the message.
        if (ex.InnerException is { } inner && inner.GetType().FullName == "Microsoft.Data.Sqlite.SqliteException")
        {
            return inner.Message;
        }
        return "<unknown>";
    }

    /// <summary>
    /// Returns true if the given <see cref="DbUpdateException"/> wraps a backend-level unique-
    /// constraint violation. Postgres → SqlState "23505"; SQLite → SqliteErrorCode 19.
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            return true;
        }
        // Match SQLite by type name so this assembly doesn't take a dep on Microsoft.Data.Sqlite.
        // Test backend (SQLite in-memory) raises SqliteException with SqliteErrorCode 19 on UNIQUE
        // constraint violations.
        var inner = ex.InnerException;
        if (inner is not null && inner.GetType().FullName == "Microsoft.Data.Sqlite.SqliteException")
        {
            var prop = inner.GetType().GetProperty("SqliteErrorCode");
            if (prop?.GetValue(inner) is int code && code == 19) return true;
        }
        return false;
    }

    public async Task MergeAnonymousViewerInto(long anonymousViewerId, long targetViewerId)
    {
        if (anonymousViewerId == targetViewerId) return;

        var anon = await _dbContext.Set<Models.Viewer>()
            .FirstOrDefaultAsync(v => v.Id == anonymousViewerId);
        if (anon is null) return;

        var target = await _dbContext.Set<Models.Viewer>()
            .FirstOrDefaultAsync(v => v.Id == targetViewerId)
            ?? throw new InvalidOperationException(
                $"Cannot merge anonymous viewer {anonymousViewerId}: target viewer {targetViewerId} not found.");

        // Two saves: free the UDID slot on the anonymous viewer first (drops the unique-index
        // conflict), then reassign to the target and delete the anonymous row in the second
        // save. The partial-failure mode (first save succeeds, second fails) leaves a benign
        // null-UDID viewer that no client can resolve to — never two rows contending for the
        // same UDID, which is the failure we actually need to prevent.
        Guid? freedUdid = anon.Udid;
        anon.Udid = null;
        await _dbContext.SaveChangesAsync();

        target.Udid = freedUdid;
        _dbContext.Set<Models.Viewer>().Remove(anon);
        await _dbContext.SaveChangesAsync();
    }

    public async Task LinkSteamToViewer(long viewerId, ulong steamId)
    {
        var viewer = await _dbContext.Set<Models.Viewer>()
            .Include(v => v.SocialAccountConnections)
            .FirstOrDefaultAsync(v => v.Id == viewerId)
            ?? throw new InvalidOperationException($"Viewer {viewerId} not found for Steam link.");

        bool alreadyLinked = viewer.SocialAccountConnections.Any(sac =>
            sac.AccountType == SocialAccountType.Steam && sac.AccountId == steamId);
        if (alreadyLinked) return;

        viewer.SocialAccountConnections.Add(new SocialAccountConnection
        {
            AccountId = steamId,
            AccountType = SocialAccountType.Steam
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Models.Viewer?> LoadForMatchContextAsync(long viewerId)
    {
        return await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .Include(v => v.Info.SelectedEmblem)
            .Include(v => v.Info.SelectedDegree)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
    }

    public Task<Models.Viewer?> LoadForBattleXpGrantAsync(long viewerId, CancellationToken ct = default)
    {
        return _dbContext.Set<Models.Viewer>()
            .Include(v => v.Classes)
                .ThenInclude(c => c.Class)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
    }

    public Task<Models.Viewer?> LoadForRankProgressAsync(long viewerId, CancellationToken ct = default)
    {
        return _dbContext.Set<Models.Viewer>()
            .Include(v => v.Classes)
                .ThenInclude(c => c.Class)
            .Include(v => v.RankProgress)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
    }

    public async Task SetGuildIdAsync(long viewerId, int guildId, CancellationToken ct = default)
    {
        var viewer = await _dbContext.Set<Models.Viewer>()
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return;
        viewer.GuildId = guildId;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task ClearGuildIdAsync(long viewerId, CancellationToken ct = default)
    {
        var viewer = await _dbContext.Set<Models.Viewer>()
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return;
        viewer.GuildId = null;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<long, string>> LoadDisplayNamesAsync(IReadOnlyCollection<long> viewerIds, CancellationToken ct = default)
    {
        if (viewerIds.Count == 0) return new Dictionary<long, string>();
        return await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .Where(v => viewerIds.Contains(v.Id))
            .Select(v => new { v.Id, v.DisplayName })
            .ToDictionaryAsync(v => v.Id, v => v.DisplayName, ct);
    }

    public async Task<IReadOnlyDictionary<long, ChatUserProfile>> LoadChatProfilesAsync(IReadOnlyCollection<long> viewerIds, CancellationToken ct = default)
    {
        if (viewerIds.Count == 0) return new Dictionary<long, ChatUserProfile>();
        var rows = await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .Include(v => v.Info.SelectedEmblem)
            .Include(v => v.Info.SelectedDegree)
            .Where(v => viewerIds.Contains(v.Id))
            .Select(v => new
            {
                v.Id,
                v.DisplayName,
                EmblemId    = v.Info.SelectedEmblem != null ? v.Info.SelectedEmblem.Id : 100_000_000L,
                CountryCode = v.Info.CountryCode ?? "",
                DegreeId    = v.Info.SelectedDegree != null ? (int)v.Info.SelectedDegree.Id : 0,
            })
            .ToListAsync(ct);
        return rows.ToDictionary(
            r => r.Id,
            r => new ChatUserProfile(
                Name:        r.DisplayName,
                EmblemId:    r.EmblemId,
                CountryCode: r.CountryCode,
                Rank:        1, // TODO: real rank when rank tracking lands
                DegreeId:    r.DegreeId));
    }

    public async Task<IReadOnlyDictionary<long, GuildMemberProfile>> LoadGuildProfileBatchAsync(IReadOnlyCollection<long> viewerIds, CancellationToken ct = default)
    {
        if (viewerIds.Count == 0) return new Dictionary<long, GuildMemberProfile>();
        var rows = await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .Include(v => v.Info.SelectedEmblem)
            .Include(v => v.Info.SelectedDegree)
            .Where(v => viewerIds.Contains(v.Id))
            .Select(v => new
            {
                v.Id,
                v.DisplayName,
                EmblemId               = v.Info.SelectedEmblem != null ? v.Info.SelectedEmblem.Id : 100_000_000L,
                CountryCode            = v.Info.CountryCode ?? "",
                DegreeId               = v.Info.SelectedDegree != null ? (int)v.Info.SelectedDegree.Id : 0,
                IsOfficialMarkDisplayed = v.Info.IsOfficialMarkDisplayed,
            })
            .ToListAsync(ct);
        return rows.ToDictionary(
            r => r.Id,
            r => new GuildMemberProfile(
                Name:                   r.DisplayName,
                EmblemId:               r.EmblemId,
                CountryCode:            r.CountryCode,
                Rank:                   1, // TODO: real rank when rank tracking lands
                DegreeId:               r.DegreeId,
                IsOfficialMarkDisplayed: r.IsOfficialMarkDisplayed));
    }

    public async Task<long> GetEquippedEmblemIdAsync(long viewerId, CancellationToken ct = default)
    {
        var row = await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .Include(v => v.Info.SelectedEmblem)
            .Where(v => v.Id == viewerId)
            .Select(v => new { EmblemId = v.Info.SelectedEmblem != null ? v.Info.SelectedEmblem.Id : 100_000_000L })
            .FirstOrDefaultAsync(ct);
        return row?.EmblemId ?? 100_000_000L;
    }

    public async Task<List<long>> GetEmblemListAsync(long viewerId, CancellationToken ct = default)
    {
        var viewer = await _dbContext.Set<Models.Viewer>()
            .AsNoTracking()
            .Include(v => v.Emblems)
            .Where(v => v.Id == viewerId)
            .FirstOrDefaultAsync(ct);
        return viewer?.Emblems.Select(e => (long)e.Id).ToList() ?? new List<long>();
    }

    public Task<int> CountUnclaimedPresentsAsync(long viewerId, CancellationToken ct = default)
    {
        return _dbContext.Set<ViewerPresent>()
            .Where(p => p.ViewerId == viewerId && p.Status == PresentStatus.Unclaimed)
            .CountAsync(ct);
    }

    private async Task<Models.Viewer> BuildDefaultViewer(string displayName, int initialTutorialState = 1)
    {
        Models.Viewer viewer = new Models.Viewer
        {
            DisplayName = displayName
        };
        var player = _config.Get<PlayerConfig>();
        var grants = _config.Get<DefaultGrantsConfig>();
        var loadout = _config.Get<DefaultLoadoutConfig>();

        viewer.Info.MaxFriends = player.MaxFriends;
        viewer.Info.CountryCode = "KOR";
        viewer.Info.BirthDate = DateTime.UtcNow;
        viewer.Currency.Crystals = grants.Crystals;
        viewer.Currency.Rupees = grants.Rupees;
        viewer.Currency.RedEther = grants.Ether;
        // TUTORIAL_STEP0 (= 1) is the fresh-signup default — see RegisterAnonymousViewer for
        // why step==0 is unsafe. RegisterViewer (admin-import + Steam-social) passes 100 so
        // those callers land at the post-tutorial baseline; import requests can still override
        // via the explicit ImportViewerRequest.TutorialState field.
        viewer.MissionData.TutorialState = initialTutorialState;

        // Load classes WITH their LeaderSkins — DefaultLeaderSkin iterates the nav collection
        // and would otherwise be null (audit §6 #3 latent NRE — this is the one).
        List<ClassEntry> classes = await _dbContext.Set<ClassEntry>()
            .Include(c => c.LeaderSkins)
            .ToListAsync();

        viewer.Classes = classes.Select(ce =>
        {
            var skin = ce.DefaultLeaderSkin ?? ce.LeaderSkins.FirstOrDefault();
            return new ViewerClassData
            {
                Class = ce,
                Exp = 0,
                // Client unconditionally indexes `_classCharaExpList[level - 1]` in
                // RankMatchUI.onOpen → CharacterExps.GetClassCharacterExps; level 0 throws IOOR.
                Level = 1,
                LeaderSkin = skin ?? new LeaderSkinEntry { Id = 0, Name = "<missing>", ClassId = ce.Id }
            };
        }).ToList();

        var defaultSleeveId = loadout.SleeveId;
        var defaultDegreeId = loadout.DegreeId;
        var defaultEmblemId = loadout.EmblemId;
        var defaultBgId = loadout.MyPageBackgroundId;
        var defaultSleeve = await _dbContext.Set<SleeveEntry>().FindAsync(defaultSleeveId);
        var defaultDegree = await _dbContext.Set<DegreeEntry>().FindAsync(defaultDegreeId);
        var defaultEmblem = await _dbContext.Set<EmblemEntry>().FindAsync(defaultEmblemId);
        var defaultBg = await _dbContext.Set<MyPageBackgroundEntry>().FindAsync(defaultBgId);
        if (defaultSleeve is not null) viewer.Sleeves.Add(defaultSleeve);
        if (defaultDegree is not null)
        {
            viewer.Degrees.Add(defaultDegree);
            viewer.Info.SelectedDegree = defaultDegree;
        }
        if (defaultEmblem is not null)
        {
            viewer.Emblems.Add(defaultEmblem);
            viewer.Info.SelectedEmblem = defaultEmblem;
        }
        if (defaultBg is not null) viewer.MyPageBackgrounds.Add(defaultBg);

        // Grant one of each class's default leader skin. Filter out the synthetic placeholders
        // (Id=0) and dedupe — skins are many-to-many via SleeveEntryViewer-style join.
        var grantedSkins = viewer.Classes
            .Select(vcd => vcd.LeaderSkin)
            .Where(s => s.Id != 0)
            .DistinctBy(s => s.Id)
            .ToList();
        viewer.LeaderSkins.AddRange(grantedSkins);

        return viewer;
    }
}
