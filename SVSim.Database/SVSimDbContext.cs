using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SVSim.Database.Common;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Entities.Story;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;

namespace SVSim.Database;

public class SVSimDbContext : DbContext
{
    private readonly ILogger<SVSimDbContext> _logger;

    public SVSimDbContext(ILogger<SVSimDbContext> logger, DbContextOptions<SVSimDbContext> options) : base(options)
    {
        _logger = logger;
    }

    #region DbSets

    public DbSet<Viewer> Viewers => Set<Viewer>();

    public DbSet<ShadowverseCardEntry> Cards => Set<ShadowverseCardEntry>();
    public DbSet<ShadowverseCardSetEntry> CardSets => Set<ShadowverseCardSetEntry>();
    public DbSet<ShadowverseDeckEntry> Decks => Set<ShadowverseDeckEntry>();
    public DbSet<CardCosmeticReward> CardCosmeticRewards => Set<CardCosmeticReward>();

    public DbSet<ClassEntry> Classes => Set<ClassEntry>();
    public DbSet<ClassExpEntry> ClassExpCurve => Set<ClassExpEntry>();
    public DbSet<LeaderSkinEntry> LeaderSkins => Set<LeaderSkinEntry>();
    public DbSet<SleeveEntry> Sleeves => Set<SleeveEntry>();
    public DbSet<EmblemEntry> Emblems => Set<EmblemEntry>();
    public DbSet<DegreeEntry> Degrees => Set<DegreeEntry>();
    public DbSet<MyPageBackgroundEntry> MyPageBackgrounds => Set<MyPageBackgroundEntry>();
    public DbSet<BattlefieldEntry> Battlefields => Set<BattlefieldEntry>();
    public DbSet<RankInfoEntry> RankInfo => Set<RankInfoEntry>();
    public DbSet<ItemEntry> Items => Set<ItemEntry>();

    public DbSet<GameConfigSection> GameConfigs => Set<GameConfigSection>();

    // Prod-captured globals — populated by SVSim.Bootstrap, not HasData. See
    // docs/audits/prod-data-capture-strategy-2026-05-23.md.
    public DbSet<MyRotationSettingEntry> MyRotationSettings => Set<MyRotationSettingEntry>();
    public DbSet<MyRotationAbilityEntry> MyRotationAbilities => Set<MyRotationAbilityEntry>();
    public DbSet<AvatarAbilityEntry> AvatarAbilities => Set<AvatarAbilityEntry>();
    public DbSet<DefaultDeckEntry> DefaultDecks => Set<DefaultDeckEntry>();
    public DbSet<ArenaSeasonConfig> ArenaSeasons => Set<ArenaSeasonConfig>();
    public DbSet<SpotCardEntry> SpotCards => Set<SpotCardEntry>();
    public DbSet<ReprintedCardEntry> ReprintedCards => Set<ReprintedCardEntry>();
    public DbSet<UnlimitedRestrictionEntry> UnlimitedRestrictions => Set<UnlimitedRestrictionEntry>();
    public DbSet<LoadingExclusionCardEntry> LoadingExclusionCards => Set<LoadingExclusionCardEntry>();
    public DbSet<BattlePassLevelEntry> BattlePassLevels => Set<BattlePassLevelEntry>();
    public DbSet<BattlePassSeasonEntry> BattlePassSeasons => Set<BattlePassSeasonEntry>();
    public DbSet<BattlePassRewardEntry> BattlePassRewards => Set<BattlePassRewardEntry>();
    public DbSet<ViewerBattlePassProgressEntry> ViewerBattlePassProgress => Set<ViewerBattlePassProgressEntry>();
    public DbSet<ViewerBattlePassClaimEntry> ViewerBattlePassClaims => Set<ViewerBattlePassClaimEntry>();
    public DbSet<MissionCatalogEntry> MissionCatalog => Set<MissionCatalogEntry>();
    public DbSet<AchievementCatalogEntry> AchievementCatalog => Set<AchievementCatalogEntry>();
    public DbSet<BattlePassMonthlyMissionEntry> BattlePassMonthlyMissions => Set<BattlePassMonthlyMissionEntry>();
    public DbSet<ViewerMission> ViewerMissions => Set<ViewerMission>();
    public DbSet<ViewerAchievement> ViewerAchievements => Set<ViewerAchievement>();
    public DbSet<ViewerEventCounter> ViewerEventCounters => Set<ViewerEventCounter>();
    public DbSet<BannerEntry> Banners => Set<BannerEntry>();
    public DbSet<HomeDialogEntry> HomeDialogEntries => Set<HomeDialogEntry>();
    public DbSet<SealedConfig> SealedSeasons => Set<SealedConfig>();
    public DbSet<MasterPointRankingPeriodEntry> MasterPointRankingPeriods => Set<MasterPointRankingPeriodEntry>();
    public DbSet<SpecialDeckFormatEntry> SpecialDeckFormats => Set<SpecialDeckFormatEntry>();
    public DbSet<PaymentItemEntry> PaymentItems => Set<PaymentItemEntry>();
    public DbSet<PackConfigEntry> Packs => Set<PackConfigEntry>();
    public DbSet<PackDrawConfigEntry> PackDrawConfigs => Set<PackDrawConfigEntry>();
    public DbSet<PackDrawSlotRateEntry> PackDrawSlotRates => Set<PackDrawSlotRateEntry>();
    public DbSet<PackDrawCardWeightEntry> PackDrawCardWeights => Set<PackDrawCardWeightEntry>();
    public DbSet<BuildDeckSeriesEntry> BuildDeckSeries => Set<BuildDeckSeriesEntry>();
    public DbSet<BuildDeckProductEntry> BuildDeckProducts => Set<BuildDeckProductEntry>();
    public DbSet<StoryDeckEntry> StoryDecks => Set<StoryDeckEntry>();
    public DbSet<SleeveShopSeriesEntry> SleeveShopSeries => Set<SleeveShopSeriesEntry>();
    public DbSet<SleeveShopProductEntry> SleeveShopProducts => Set<SleeveShopProductEntry>();
    public DbSet<ItemPurchaseCatalogEntry> ItemPurchaseCatalog => Set<ItemPurchaseCatalogEntry>();
    public DbSet<LeaderSkinShopSeriesEntry> LeaderSkinShopSeries => Set<LeaderSkinShopSeriesEntry>();
    public DbSet<LeaderSkinShopProductEntry> LeaderSkinShopProducts => Set<LeaderSkinShopProductEntry>();
    public DbSet<ViewerLeaderSkinSetClaim> ViewerLeaderSkinSetClaims => Set<ViewerLeaderSkinSetClaim>();
    public DbSet<SpotCardExchangeEntry> SpotCardExchangeCatalog => Set<SpotCardExchangeEntry>();
    public DbSet<ViewerSpotCardExchange> ViewerSpotCardExchanges => Set<ViewerSpotCardExchange>();
    public DbSet<MaintenanceCardEntry> MaintenanceCards => Set<MaintenanceCardEntry>();
    public DbSet<FeatureMaintenanceEntry> FeatureMaintenances => Set<FeatureMaintenanceEntry>();
    public DbSet<PreReleaseInfo> PreReleaseInfos => Set<PreReleaseInfo>();
    public DbSet<PracticeOpponentEntry> PracticeOpponents => Set<PracticeOpponentEntry>();
    public DbSet<BotRosterEntry> BotRoster => Set<BotRosterEntry>();
    public DbSet<PuzzleGroupEntry> PuzzleGroups => Set<PuzzleGroupEntry>();
    public DbSet<PuzzleEntry> Puzzles => Set<PuzzleEntry>();
    public DbSet<PuzzleMissionEntry> PuzzleMissions => Set<PuzzleMissionEntry>();
    public DbSet<ViewerPuzzleClear> ViewerPuzzleClears => Set<ViewerPuzzleClear>();

    // Story reference data + viewer progress
    public DbSet<StoryWorld> StoryWorlds => Set<StoryWorld>();
    public DbSet<StorySection> StorySections => Set<StorySection>();
    public DbSet<StoryChapter> StoryChapters => Set<StoryChapter>();
    public DbSet<SpecialBattleSetting> SpecialBattleSettings => Set<SpecialBattleSetting>();
    public DbSet<ViewerStoryProgress> ViewerStoryProgress => Set<ViewerStoryProgress>();
    public DbSet<ViewerStoryBranchUnlock> ViewerStoryBranchUnlocks => Set<ViewerStoryBranchUnlock>();

    public DbSet<ViewerPresent> ViewerPresents => Set<ViewerPresent>();
    public DbSet<TutorialPresentEntry> TutorialPresentEntries => Set<TutorialPresentEntry>();
    public DbSet<ViewerAcquireHistoryEntry> ViewerAcquireHistory => Set<ViewerAcquireHistoryEntry>();

    public DbSet<ArenaTwoPickReward> ArenaTwoPickRewards { get; set; } = null!;
    public DbSet<ViewerArenaTwoPickRun> ViewerArenaTwoPickRuns { get; set; } = null!;
    public DbSet<ViewerArenaColosseumRun> ViewerArenaColosseumRuns { get; set; } = null!;
    public DbSet<ColosseumHofDeck> ColosseumHofDecks { get; set; } = null!;
    public DbSet<ColosseumWindFallDeck> ColosseumWindFallDecks { get; set; } = null!;
    public DbSet<ColosseumAvatarDeck> ColosseumAvatarDecks { get; set; } = null!;

    public DbSet<SerialCodeEntry> SerialCodes => Set<SerialCodeEntry>();
    public DbSet<SerialCodeRewardEntry> SerialCodeRewards => Set<SerialCodeRewardEntry>();
    public DbSet<ViewerSerialCodeRedemption> ViewerSerialCodeRedemptions => Set<ViewerSerialCodeRedemption>();

    public DbSet<ViewerFriend> ViewerFriends => Set<ViewerFriend>();
    public DbSet<ViewerFriendApply> ViewerFriendApplies => Set<ViewerFriendApply>();
    public DbSet<ViewerPlayedTogether> ViewerPlayedTogethers => Set<ViewerPlayedTogether>();
    public DbSet<ViewerBattleHistory> ViewerBattleHistories => Set<ViewerBattleHistory>();

    public DbSet<Guild> Guilds => Set<Guild>();
    public DbSet<GuildMember> GuildMembers => Set<GuildMember>();
    public DbSet<GuildInvite> GuildInvites => Set<GuildInvite>();
    public DbSet<GuildJoinRequest> GuildJoinRequests => Set<GuildJoinRequest>();
    public DbSet<GuildChatMessage> GuildChatMessages => Set<GuildChatMessage>();

    #endregion

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entityEntry in ChangeTracker.Entries())
        {
            if (entityEntry.Entity is ITimeTrackedEntity timeTrackedEntity)
            {
                if (entityEntry.State is EntityState.Added && timeTrackedEntity.DateCreated == DateTime.MinValue)
                {
                    timeTrackedEntity.DateCreated = DateTime.UtcNow;
                }
                if (entityEntry.State is EntityState.Modified or EntityState.Added)
                {
                    timeTrackedEntity.DateUpdated = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShadowverseDeckEntry>()
            .OwnsMany(de => de.Cards);

        // BaseEntity<TKey> annotates Id with [DatabaseGenerated(None)] for the integer-PK
        // entities seeded via HasData. ShadowverseDeckEntry uses Guid and is created at
        // runtime — without client-side generation every new deck gets Guid.Empty and the
        // second deck insert collides on PK. (DDL has no column default; this only works
        // because EF generates a sequential Guid before INSERT.)
        modelBuilder.Entity<ShadowverseDeckEntry>()
            .Property(d => d.Id)
            .ValueGeneratedOnAdd();

        // EF can't figure this many-to-many out on its own
        modelBuilder.Entity<SleeveEntry>()
            .HasMany(se => se.Viewers)
            .WithMany(v => v.Sleeves);

        modelBuilder.HasSequence<long>("ShortUdidSequence").StartsAt(400000000);
        modelBuilder.Entity<Viewer>()
            .Property(v => v.ShortUdid)
            .UseSequence("ShortUdidSequence");

        modelBuilder.Entity<PackConfigEntry>().OwnsMany(p => p.ChildGachas);
        modelBuilder.Entity<PackConfigEntry>().OwnsMany(p => p.Banners);

        modelBuilder.Entity<PackDrawSlotRateEntry>(e =>
        {
            e.HasIndex(x => new { x.PackId, x.Slot, x.Tier }).IsUnique();
        });
        modelBuilder.Entity<PackDrawCardWeightEntry>(e =>
        {
            e.HasIndex(x => new { x.PackId, x.Slot, x.Tier });
        });
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.PackOpenCounts);
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.PackStarterClasses, b =>
        {
            // One choice per (viewer, pack) — /pack/set_rotation_starter_class is one-shot
            // per pack. Mirrors the (ViewerId, PackId) unique-index pattern used by
            // GachaPointBalances above (project_owned_collection_unique_index memory).
            b.HasIndex("ViewerId", nameof(ViewerPackStarterClass.PackId)).IsUnique();
        });
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.FreePackClaims, b =>
        {
            b.WithOwner().HasForeignKey("ViewerId");
            b.HasKey("ViewerId", nameof(ViewerFreePackClaim.FreeGachaCampaignId));
            b.Property(x => x.FreeGachaCampaignId).ValueGeneratedNever();
        });
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.MyPageBgRotation, b =>
        {
            b.ToTable("ViewerMyPageBgRotation");
            b.WithOwner().HasForeignKey("ViewerId");
            b.HasKey("ViewerId", nameof(MyPageBgRotationEntry.Slot));
            b.Property(x => x.Slot).ValueGeneratedNever();
        });

        // OwnedCardEntry and OwnedItemEntry use composite PK (ViewerId, Id) where Id is auto-
        // generated, which silently permits multiple rows per (Viewer, Card) or (Viewer, Item).
        // The intended semantic is one row per pair with Count as multiplicity — enforce that as
        // a unique index so any future find-or-add that forgets to .Include the collection (and
        // therefore re-creates a row that already exists in the DB) crashes loudly at SaveChanges
        // instead of silently duplicating ownership rows.
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.Cards, b =>
        {
            b.HasIndex("ViewerId", "CardId").IsUnique();
        });
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.Items, b =>
        {
            b.HasIndex("ViewerId", "ItemId").IsUnique();
        });

        modelBuilder.Entity<Viewer>().OwnsMany(v => v.BuildDeckPurchases, b =>
        {
            b.HasIndex("ViewerId", "ProductId").IsUnique();
        });

        modelBuilder.Entity<Viewer>().OwnsMany(v => v.GachaPointBalances, b =>
        {
            b.HasIndex("ViewerId", "PackId").IsUnique();
        });

        modelBuilder.Entity<Viewer>().OwnsMany(v => v.GachaPointReceived, b =>
        {
            b.HasIndex("ViewerId", "PackId", "CardId").IsUnique();
        });

        // A given social account links to exactly one viewer — two viewers cannot share the same
        // Steam (or Facebook, etc.) account. This is the dedup backstop the auth handler's find-
        // or-link path (SteamSessionAuthenticationHandler) relies on: two concurrent first-Steam-
        // touch requests can both pass the .Any(...) check in LinkSteamToViewer, but the second
        // SaveChanges() throws unique-violation and surfaces a clean 500 instead of silently
        // appending duplicate connections.
        modelBuilder.Entity<Viewer>().OwnsMany(v => v.SocialAccountConnections, b =>
        {
            b.HasIndex("AccountType", "AccountId").IsUnique();
        });

        modelBuilder.Entity<BuildDeckSeriesEntry>().OwnsMany(s => s.SeriesRewards);
        modelBuilder.Entity<BuildDeckProductEntry>().OwnsMany(p => p.Cards);
        modelBuilder.Entity<BuildDeckProductEntry>().OwnsMany(p => p.Rewards);

        modelBuilder.Entity<BuildDeckProductEntry>()
            .HasOne(p => p.Series)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BuildDeckProductEntry>().HasIndex(p => p.SeriesId);

        modelBuilder.Entity<SleeveShopProductEntry>().OwnsMany(p => p.Rewards);
        modelBuilder.Entity<SleeveShopProductEntry>()
            .HasOne(p => p.Series)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SleeveShopProductEntry>().HasIndex(p => p.SeriesId);

        modelBuilder.Entity<LeaderSkinShopSeriesEntry>().OwnsMany(s => s.SetCompletionRewards);
        modelBuilder.Entity<LeaderSkinShopProductEntry>().OwnsMany(p => p.Rewards);
        modelBuilder.Entity<LeaderSkinShopProductEntry>()
            .HasOne(p => p.Series)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LeaderSkinShopProductEntry>().HasIndex(p => p.SeriesId);

        modelBuilder.Entity<ViewerLeaderSkinSetClaim>(b =>
        {
            b.HasKey(c => new { c.ViewerId, c.SeriesId });
            b.HasIndex(c => c.ViewerId);
        });

        modelBuilder.Entity<ViewerSpotCardExchange>(b =>
        {
            b.HasKey(e => new { e.ViewerId, e.CardId });
            b.HasIndex(e => e.ViewerId);
        });

        modelBuilder.Entity<CardCosmeticReward>(b =>
        {
            b.HasKey(r => new { r.CardId, r.Type, r.CosmeticId });
            b.HasIndex(r => r.CardId);
            // No inverse nav on the Card side — avoid forcing CosmeticRewards to load on every
            // Card query. See project_ef_split_query memory for the cartesian-explode risk.
            b.HasOne(r => r.Card)
                .WithMany()
                .HasForeignKey(r => r.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GameConfigSection: one row per top-level config section. Postgres stores ValueJson as
        // jsonb (gives jsonb-side queryability if needed later); SQLite gets a plain TEXT column.
        // EF never sees the section POCO shapes — IGameConfigService owns deserialisation via STJ.
        // Replaces the old single-row GameConfigurations table with its EF Core 8 OwnsOne+ToJson
        // tree; see 2026-05-24 config refactor.
        bool isPostgres = Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;
        if (isPostgres)
        {
            modelBuilder.Entity<GameConfigSection>()
                .Property(s => s.ValueJson)
                .HasColumnType("jsonb");
        }

        // --- Story entities ---

        // Composite PKs for viewer-state tables
        modelBuilder.Entity<ViewerStoryProgress>().HasKey(x => new { x.ViewerId, x.StoryId });
        modelBuilder.Entity<ViewerStoryBranchUnlock>().HasKey(x => new { x.ViewerId, x.StoryId });

        // StoryChapter owned collections (shadow-PK per row)
        modelBuilder.Entity<StoryChapter>(c =>
        {
            c.OwnsMany(x => x.BattleSettings, b => { b.WithOwner().HasForeignKey("StoryId"); b.Property<int>("Id"); b.HasKey("StoryId", "Id"); });
            c.OwnsMany(x => x.Rewards,        b => { b.WithOwner().HasForeignKey("StoryId"); b.Property<int>("Id"); b.HasKey("StoryId", "Id"); });
            c.OwnsMany(x => x.SubChapters,    b => { b.WithOwner().HasForeignKey("StoryId"); b.Property<int>("Id"); b.HasKey("StoryId", "Id"); });
        });

        // FK relationships
        modelBuilder.Entity<StorySection>().HasOne(s => s.World).WithMany().HasForeignKey(s => s.WorldId);
        modelBuilder.Entity<StoryChapter>().HasOne(c => c.Section).WithMany().HasForeignKey(c => c.SectionId);
        modelBuilder.Entity<StoryChapter>().HasOne(c => c.SpecialBattleSetting).WithMany().HasForeignKey(c => c.SpecialBattleSettingId);

        // Indexes
        modelBuilder.Entity<StoryChapter>().HasIndex(c => new { c.SectionId, c.CharaId, c.ChapterId });
        modelBuilder.Entity<StoryChapter>().HasIndex(c => c.NextChapterId);

        // --- Battle pass entities ---

        modelBuilder.Entity<BattlePassSeasonEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedNever();
            b.HasIndex(e => new { e.StartDate, e.EndDate });
            b.HasMany(e => e.Rewards).WithOne(r => r.Season).HasForeignKey(r => r.SeasonId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BattlePassRewardEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.SeasonId, e.Track, e.Level }).IsUnique();
        });

        modelBuilder.Entity<ViewerBattlePassProgressEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.HasIndex(e => new { e.ViewerId, e.SeasonId }).IsUnique();
        });

        modelBuilder.Entity<ViewerBattlePassClaimEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.HasIndex(e => new { e.ViewerId, e.SeasonId, e.Track, e.Level }).IsUnique();
            b.HasIndex(e => new { e.ViewerId, e.SeasonId });
        });

        modelBuilder.Entity<MissionCatalogEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedNever();
            b.HasIndex(e => e.LotType);
            b.HasIndex(e => new { e.EventType, e.EventArg });
        });

        modelBuilder.Entity<AchievementCatalogEntry>(b =>
        {
            b.HasKey(e => new { e.AchievementType, e.Level });
            b.HasIndex(e => e.AchievementType);
            b.HasIndex(e => new { e.EventType, e.EventArg });
        });

        modelBuilder.Entity<BattlePassMonthlyMissionEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.Year, e.Month, e.OrderNum }).IsUnique();
            b.HasIndex(e => new { e.Year, e.Month });
        });

        modelBuilder.Entity<ViewerMission>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.ViewerId, e.Slot }).IsUnique();
            b.HasIndex(e => e.ViewerId);
        });

        modelBuilder.Entity<ViewerAchievement>(b =>
        {
            b.HasKey(e => new { e.ViewerId, e.AchievementType });
        });

        modelBuilder.Entity<ViewerEventCounter>(b =>
        {
            b.HasKey(e => new { e.ViewerId, e.EventKey, e.Period });
            b.HasIndex(e => new { e.ViewerId, e.Period });
        });

        modelBuilder.Entity<ViewerPresent>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.PresentId).HasMaxLength(64);
            b.Property(p => p.Source).HasMaxLength(64);

            b.HasOne(p => p.Viewer)
                .WithMany()
                .HasForeignKey(p => p.ViewerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Drives /gift/top — partition by status, then chronological.
            b.HasIndex(p => new { p.ViewerId, p.Status, p.CreatedAt });

            // One row per (viewer, present_id) — backstop against double-seeding and
            // double-enqueue from future producers.
            b.HasIndex(p => new { p.ViewerId, p.PresentId }).IsUnique();
        });

        modelBuilder.Entity<TutorialPresentEntry>(b =>
        {
            b.HasKey(p => p.PresentId);
            b.Property(p => p.PresentId).HasMaxLength(64);
        });

        modelBuilder.Entity<ViewerAcquireHistoryEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Message).HasMaxLength(64);
            b.HasOne<Viewer>()
                .WithMany()
                .HasForeignKey(e => e.ViewerId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => new { e.ViewerId, e.AcquireTime, e.Id });
        });

        modelBuilder.Entity<SerialCodeEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Code).HasMaxLength(64).IsRequired();
            b.Property(e => e.Message).HasMaxLength(255);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasMany(e => e.Rewards)
                .WithOne()
                .HasForeignKey(r => r.SerialCodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SerialCodeRewardEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.HasIndex(e => new { e.SerialCodeId, e.Slot });
        });

        modelBuilder.Entity<ViewerSerialCodeRedemption>(b =>
        {
            b.HasKey(e => new { e.ViewerId, e.SerialCodeId });
            b.HasOne<Viewer>()
                .WithMany()
                .HasForeignKey(e => e.ViewerId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<SerialCodeEntry>()
                .WithMany()
                .HasForeignKey(e => e.SerialCodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ViewerFriend>(b =>
        {
            b.HasKey(e => new { e.OwnerViewerId, e.FriendViewerId });
            b.HasIndex(e => new { e.OwnerViewerId, e.CreatedAt });
            b.HasOne<Viewer>().WithMany().HasForeignKey(e => e.OwnerViewerId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Viewer>().WithMany().HasForeignKey(e => e.FriendViewerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ViewerFriendApply>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.FromViewerId, e.ToViewerId }).IsUnique();
            b.HasIndex(e => e.ToViewerId);
            b.HasOne<Viewer>().WithMany().HasForeignKey(e => e.FromViewerId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Viewer>().WithMany().HasForeignKey(e => e.ToViewerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ViewerPlayedTogether>(b =>
        {
            b.HasKey(e => new { e.OwnerViewerId, e.OpponentViewerId });
            b.HasIndex(e => new { e.OwnerViewerId, e.PlayedAt });
            b.HasOne<Viewer>().WithMany().HasForeignKey(e => e.OwnerViewerId).OnDelete(DeleteBehavior.Cascade);
            // OpponentViewerId is NOT an FK — we want survivors' history to outlive a deleted opponent.
        });

        modelBuilder.Entity<ViewerBattleHistory>(b =>
        {
            b.HasKey(e => new { e.ViewerId, e.BattleId });
            b.HasIndex(e => new { e.ViewerId, e.CreateTime })
                .HasDatabaseName("IX_ViewerBattleHistories_ViewerId_CreateTime");
            b.Property(e => e.SelfRotationId).IsRequired();
            b.Property(e => e.OpponentName).IsRequired();
            b.Property(e => e.OpponentCountryCode).IsRequired();
            b.Property(e => e.OpponentRotationId).IsRequired();
        });

        // Colosseum curated-deck pools — DeckNo is the wire identifier admins reference in
        // register endpoints; uniqueness per pool is the contract clients rely on.
        modelBuilder.Entity<ColosseumHofDeck>().HasIndex(d => d.DeckNo).IsUnique();
        modelBuilder.Entity<ColosseumWindFallDeck>().HasIndex(d => d.DeckNo).IsUnique();
        modelBuilder.Entity<ColosseumAvatarDeck>().HasIndex(d => d.DeckNo).IsUnique();

        // --- Guild entities ---

        modelBuilder.Entity<Guild>(e =>
        {
            e.HasKey(g => g.GuildId);
            e.Property(g => g.GuildId).ValueGeneratedNever();     // we pick the random id ourselves
            e.Property(g => g.Name).HasMaxLength(64);
            e.Property(g => g.Description).HasMaxLength(512);
            e.HasIndex(g => g.Name);                              // name search
            e.HasOne<Viewer>().WithMany()
                .HasForeignKey(g => g.LeaderViewerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<GuildMember>(e =>
        {
            e.HasKey(m => new { m.GuildId, m.ViewerId });
            e.HasOne(m => m.Guild).WithMany(g => g.Members).HasForeignKey(m => m.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => m.ViewerId).IsUnique();               // one-guild-per-viewer at DB level
        });

        modelBuilder.Entity<GuildInvite>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).ValueGeneratedOnAdd();
            e.HasOne(i => i.Guild).WithMany().HasForeignKey(i => i.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(i => new { i.GuildId, i.InviteeViewerId }).IsUnique()
                .HasFilter("\"Status\" = 0");                                  // partial: one PENDING invite per pair
            e.HasIndex(i => new { i.InviteeViewerId, i.Status });             // "my pending invites"
        });

        modelBuilder.Entity<GuildJoinRequest>(e =>
        {
            e.HasKey(r => new { r.GuildId, r.ViewerId });
            e.HasOne(r => r.Guild).WithMany().HasForeignKey(r => r.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => new { r.ViewerId, r.Status });
            e.HasIndex(r => new { r.GuildId, r.Status });
        });

        modelBuilder.Entity<GuildChatMessage>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Guild).WithMany().HasForeignKey(c => c.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => new { c.GuildId, c.MessageId }).IsUnique();
            e.Property(c => c.DeckPayload).HasColumnType("jsonb");
            e.Property(c => c.ReplayPayload).HasColumnType("jsonb");
            e.Property(c => c.RoomPayload).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Viewer>(e =>
        {
            e.HasOne(v => v.Guild).WithMany().HasForeignKey(v => v.GuildId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }

    public void UpdateDatabase()
    {
        IEnumerable<string> pendingMigrations = Database.GetPendingMigrations();
        if (!pendingMigrations.Any())
        {
            _logger.LogDebug("No pending migrations found, continuing.");
            return;
        }

        foreach (string migration in pendingMigrations)
        {
            _logger.LogInformation("Found pending migration with name {migrationName}.", migration);
        }
        _logger.LogInformation("Attempting to apply pending migrations...");
        Database.Migrate();
        _logger.LogInformation("Migrations applied.");
    }

    /// <summary>
    /// Idempotent runtime seed for entities that can't use HasData. For GameConfigSection: walks
    /// every <see cref="ConfigSectionAttribute"/>-marked POCO in the Models.Config namespace and
    /// inserts a row containing its <c>ShippedDefaults()</c> payload if no row for that section
    /// name exists. Safe to run on every startup — only missing rows are added; operator-edited
    /// rows are left alone.
    /// </summary>
    public async Task EnsureSeedDataAsync()
    {
        var existing = await GameConfigs.Select(s => s.SectionName).ToListAsync();
        var existingSet = new HashSet<string>(existing, StringComparer.Ordinal);
        int added = 0;

        foreach (var (name, json) in EnumerateShippedDefaults())
        {
            if (existingSet.Contains(name)) continue;
            GameConfigs.Add(new GameConfigSection { SectionName = name, ValueJson = json });
            added++;
        }

        if (added > 0)
        {
            await SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} default GameConfigSection row(s).", added);
        }
    }

    private static IEnumerable<(string Name, string Json)> EnumerateShippedDefaults()
    {
        // Reflect over every [ConfigSection]-marked type in the same assembly as PackRateConfig.
        // Each type must expose a parameterless `public static T ShippedDefaults()` — see the
        // POCOs in Models/Config for the convention.
        var asm = typeof(PackRateConfig).Assembly;
        var stjOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
        };

        foreach (var t in asm.GetTypes())
        {
            var attr = t.GetCustomAttributes(typeof(ConfigSectionAttribute), inherit: false)
                       .Cast<ConfigSectionAttribute>().FirstOrDefault();
            if (attr is null) continue;

            var factory = t.GetMethod("ShippedDefaults",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                binder: null, types: Type.EmptyTypes, modifiers: null);
            if (factory is null)
            {
                throw new InvalidOperationException(
                    $"[ConfigSection] type {t.FullName} is missing `public static {t.Name} ShippedDefaults()`.");
            }
            var instance = factory.Invoke(null, null)
                ?? throw new InvalidOperationException($"{t.FullName}.ShippedDefaults() returned null.");
            yield return (attr.Name, System.Text.Json.JsonSerializer.Serialize(instance, t, stjOptions));
        }
    }
}
