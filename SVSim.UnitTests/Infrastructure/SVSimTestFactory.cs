using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.BattlePass;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;

namespace SVSim.UnitTests.Infrastructure;

/// <summary>
/// Test host for the EmulatedEntrypoint app. Each instance opens a private SQLite in-memory
/// database, swaps the production DbContext + Steam auth handler for SQLite-friendly +
/// header-driven test versions, and exposes a <see cref="SeedViewerAsync"/> helper for tests
/// to create realistic viewer rows.
/// </summary>
internal class SVSimTestFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    private long _nextSeededShortUdid = 400_000_001;
    private readonly bool _freeplayEnabled;
    private readonly bool _useRealAuthHandler;

    public SVSimTestFactory(bool freeplayEnabled = false, bool useRealAuthHandler = false)
    {
        _freeplayEnabled = freeplayEnabled;
        _useRealAuthHandler = useRealAuthHandler;
        // SQLite :memory: lives only as long as a connection is open — keep ours open for the
        // factory's lifetime so the DbContext can reattach to the same DB across scopes.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Tell Program.cs we're in tests so it skips UpdateDatabase() — the Postgres-targeted
        // migrations would fail against SQLite. We call EnsureCreated below instead.
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            ReplaceDbContext(services);
            if (!_useRealAuthHandler)
            {
                ReplaceAuthHandler(services);
            }
            else
            {
                // Real auth handler stays in place; bypass the live Steam SDK so synthetic
                // tickets validate without touching Steam.
                var steamServer = services.FirstOrDefault(d => d.ServiceType == typeof(SVSim.EmulatedEntrypoint.Services.ISteamServer));
                if (steamServer is not null) services.Remove(steamServer);
                services.AddSingleton<SVSim.EmulatedEntrypoint.Services.ISteamServer,
                                      SVSim.EmulatedEntrypoint.Services.DevAlwaysValidSteamServer>();
            }
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.Database.EnsureCreated();
        db.EnsureSeedDataAsync().GetAwaiter().GetResult();

        if (_freeplayEnabled)
        {
            using var seedScope = host.Services.CreateScope();
            var seedDb = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            const string freeplayJson = "{\"Enabled\":true,\"CurrencyAmount\":99999,\"CardCopies\":3}";
            var existing = seedDb.GameConfigs.FirstOrDefault(s => s.SectionName == "Freeplay");
            if (existing is null)
                seedDb.GameConfigs.Add(new SVSim.Database.Models.GameConfigSection { SectionName = "Freeplay", ValueJson = freeplayJson });
            else
                existing.ValueJson = freeplayJson;
            seedDb.SaveChanges();
        }

        // Reference data is no longer HasData-seeded; load the CSVs via the same importer
        // production uses so tests exercise the same code path. CardCosmeticRewards skipped —
        // FK to Cards would reject every row against the minimal 3-card test seed below.
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        new ReferenceDataImporter(TextWriter.Null, TextWriter.Null)
            .ImportAllAsync(db, dataDir).GetAwaiter().GetResult();

        // Seed a minimal card set so card-pool tests can resolve a non-empty pool without
        // requiring the full CardImporter tool or a cards.json file. The set is marked
        // IsInRotation so both standard-pack (by setId) and special-pack (rotation scan)
        // tests see real data.
        SeedMinimalCardSet(db);
        SeedMinimalPackDrawTable(db);

        return host;
    }

    /// <summary>
    /// Seeds a minimal PackDrawConfig + slot rates + card weights for the test card-set's
    /// cards (10001001/10001002/10001003) under pack id 10001. Lets PackController.Open
    /// resolve a draw table without requiring tests to run the full PackDrawTableImporter.
    /// </summary>
    private static void SeedMinimalPackDrawTable(SVSimDbContext db)
    {
        if (db.PackDrawConfigs.Any())
            return;

        const int packId = 10001;
        db.PackDrawConfigs.Add(new PackDrawConfigEntry { Id = packId, AnimationRatePct = 0 });
        // Slot rates: uniform single-tier so any rng lands somewhere valid.
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.General, Tier = DrawTier.Bronze, RatePct = 100 });
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, RatePct = 100 });
        // Card weights for both slots.
        db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry { PackId = packId, Slot = DrawSlot.General, Tier = DrawTier.Bronze, CardId = 10001001, RatePct = 100 });
        db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry { PackId = packId, Slot = DrawSlot.Eighth,  Tier = DrawTier.Bronze, CardId = 10001001, RatePct = 100 });
        db.SaveChanges();
    }

    private static void SeedMinimalCardSet(SVSimDbContext db)
    {
        if (db.CardSets.Any())
            return;   // Already seeded (e.g. if CreateHost is called more than once)

        var set = new ShadowverseCardSetEntry
        {
            Id          = 10001,
            Name        = "TestSet",
            IsInRotation = true,
            IsBasic      = false,
            Cards        =
            [
                new ShadowverseCardEntry { Id = 10001001L, Name = "TestCard1", Rarity = Rarity.Bronze },
                new ShadowverseCardEntry { Id = 10001002L, Name = "TestCard2", Rarity = Rarity.Gold },
                new ShadowverseCardEntry { Id = 10001003L, Name = "TestCard3", Rarity = Rarity.Legendary },
            ]
        };
        db.CardSets.Add(set);
        db.SaveChanges();
    }

    private void ReplaceDbContext(IServiceCollection services)
    {
        // Production registered DbContextOptions<SVSimDbContext> with the Npgsql provider; tear
        // out every related descriptor so AddDbContext below installs a clean SQLite-backed one.
        foreach (var descriptor in services
                     .Where(d => d.ServiceType == typeof(DbContextOptions<SVSimDbContext>)
                              || d.ServiceType == typeof(DbContextOptions)
                              || d.ServiceType == typeof(SVSimDbContext))
                     .ToList())
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<SVSimDbContext>(opt =>
        {
            opt.UseSqlite(_connection);
            opt.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCustomizer, SqliteFriendlyModelCustomizer>();
        });
    }

    private static void ReplaceAuthHandler(IServiceCollection services)
    {
        // Production Program.cs registered SteamSessionAuthenticationHandler under the
        // "SteamAuthentication" scheme. Drop that scheme from BOTH the SchemeMap and the
        // parallel Schemes list (AddScheme writes to both — and the provider iterates the
        // list, not the map, so leaving the old builder behind throws "Scheme already exists"
        // when it re-adds during provider construction).
        services.AddTransient<TestAuthHandler>();
        services.PostConfigure<AuthenticationOptions>(opt =>
        {
            opt.SchemeMap.Remove(SteamAuthenticationConstants.SchemeName, out _);
            var schemesList = (IList<AuthenticationSchemeBuilder>)opt.Schemes;
            foreach (var stale in schemesList
                         .Where(s => s.Name == SteamAuthenticationConstants.SchemeName)
                         .ToList())
            {
                schemesList.Remove(stale);
            }

            opt.AddScheme(SteamAuthenticationConstants.SchemeName, b =>
            {
                b.HandlerType = typeof(TestAuthHandler);
            });
        });
    }

    /// <summary>
    /// Creates a fully-formed viewer via the real <see cref="IViewerRepository.RegisterViewer"/>
    /// path (so the test exercises the same nav-graph wiring real users hit). The viewer's
    /// <c>ShortUdid</c> is overwritten to a unique non-zero value because the Postgres sequence
    /// is disabled on SQLite — without this every test viewer collides on 0.
    /// </summary>
    public async Task<long> SeedViewerAsync(
        ulong steamId = 76_561_198_000_000_001UL,
        string displayName = "Test Viewer",
        int tutorialState = 100)
    {
        long viewerId;
        long shortUdid;

        using (var scope = Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            var v = await repo.RegisterViewer(displayName, SocialAccountType.Steam, steamId);
            viewerId = v.Id;
            shortUdid = Interlocked.Increment(ref _nextSeededShortUdid);
        }

        // Second scope: assign a real ShortUdid so claim-based lookups in tests have something
        // to find (and so per-viewer ShortUdids don't collide across SeedViewerAsync calls).
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
            v.ShortUdid = shortUdid;
            await db.SaveChangesAsync();
        }

        // Third scope: write the requested TutorialState. The parameter defaults to 100 —
        // the post-tutorial baseline that ~30 existing tests rely on — so callers that don't
        // care about the tutorial step keep working unchanged. Pass tutorialState: 1 to seed
        // a fresh-signup viewer, or any other value to land mid-tutorial. RegisterViewer's
        // own default (set in BuildDefaultViewer) is irrelevant here because this override
        // always runs.
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.MissionData).FirstAsync(v => v.Id == viewerId);
            viewer.MissionData.TutorialState = tutorialState;
            await db.SaveChangesAsync();
        }

        return viewerId;
    }

    /// <summary>
    /// Runs the per-domain seed importers against the test SQLite DB using the seed JSON
    /// copied into the test output dir (see SVSim.UnitTests.csproj Content Includes for
    /// Data/seeds and Data/test-fixtures/seeds). Idempotent — safe to call multiple times.
    /// Tests that depend on prod-shaped global content (spot_cards, avatar abilities, etc.)
    /// call this once during setup; the rest of the test runs against whatever the importers
    /// populated. Mirrors the wiring in <see cref="SVSim.Bootstrap.Program"/>.
    /// </summary>
    public async Task SeedGlobalsAsync()
    {
        string seedDir = Path.Combine(AppContext.BaseDirectory, "Data", "seeds");
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // RotationConfigImporter must precede RotationFlagUpdater; CardListsImporter is
        // ordered after the GameConfig importers for tidiness (no FK dependency).
        await new RotationConfigImporter().ImportAsync(ctx, seedDir);
        await new MyRotationImporter().ImportAsync(ctx, seedDir);
        await new AvatarAbilityImporter().ImportAsync(ctx, seedDir);
        await new ArenaSeasonImporter().ImportAsync(ctx, seedDir);
        await new BattlePassImporter().ImportAsync(ctx, seedDir);
        await new BattlePassSeasonImporter().ImportAsync(ctx, seedDir);
        await new BattlePassRewardImporter().ImportAsync(ctx, seedDir);
        await new PreReleaseInfoImporter().ImportAsync(ctx, seedDir);
        await new CardListsImporter().ImportAsync(ctx, seedDir);
        await new RotationFlagUpdater().UpdateAsync(ctx);

        await new PracticeOpponentImporter().ImportAsync(ctx, seedDir);
        await new BotRosterImporter().ImportAsync(ctx, seedDir);
        await new PaymentItemImporter().ImportAsync(ctx, seedDir);
        await new ItemImporter().ImportAsync(ctx, seedDir);
        await new SleeveShopImporter().ImportAsync(ctx, seedDir);
        await new ItemPurchaseImporter().ImportAsync(ctx, seedDir);
        await new LeaderSkinShopImporter().ImportAsync(ctx, seedDir);
        await new SpotCardExchangeImporter().ImportAsync(ctx, seedDir);
        var puzzleImporter = new PuzzleImporter();
        await puzzleImporter.ImportGroupsAsync(ctx, seedDir);
        await puzzleImporter.ImportPuzzlesAsync(ctx, seedDir);
        await puzzleImporter.ImportMissionsAsync(ctx, seedDir);

        var mypage = new MyPageGlobalsImporter();
        await mypage.ImportBannersAsync(ctx, seedDir);
        await mypage.ImportSealedAsync(ctx, seedDir);
        await mypage.ImportMasterPointRankingPeriodAsync(ctx, seedDir);
        await mypage.ImportSpecialDeckFormatsAsync(ctx, seedDir);
        await mypage.ImportHomeDialogsAsync(ctx, seedDir);

        await new TutorialPresentsImporter().ImportAsync(ctx, seedDir);

        await new DefaultDeckImporter().ImportAsync(ctx, seedDir);
        await new PackImporter().ImportAsync(ctx, seedDir);
        // PackDrawTableImporter is NOT called here — production draw tables reference real
        // Cygames card_ids not present in the test's minimal card master. Tests that
        // exercise /pack/open use SeedPackDrawTableAsync to install a stub draw table
        // pointing to their seeded test cards.
    }

    /// <summary>
    /// Installs a minimal PackDrawConfig + slot rates + per-card weights for <paramref name="packId"/>,
    /// pointing the per-card weights at <paramref name="cardIds"/>. All cards land in the Bronze tier
    /// at 100% rate; slot 1-7 and slot 8 both draw from the same pool. Use for tests that need
    /// /pack/open to succeed against a custom seeded card pool.
    /// </summary>
    public Task SeedPackDrawTableAsync(int packId, params long[] cardIds)
        => SeedPackDrawTableAsync(packId, DrawTier.Bronze, cardIds);

    /// <summary>
    /// Convenience for gacha-point tests: picks Legendary cards from <paramref name="cardSetId"/>
    /// (skipping foils) and seeds them as the draw table's Legendary tier for <paramref name="packId"/>.
    /// </summary>
    public async Task SeedPackDrawTableFromSetAsync(int packId, int cardSetId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var legendaryIds = await db.CardSets
            .Where(s => s.Id == cardSetId)
            .SelectMany(s => s.Cards)
            .Where(c => c.Rarity == SVSim.Database.Enums.Rarity.Legendary && !c.IsFoil)
            .Select(c => c.Id)
            .ToListAsync();

        if (legendaryIds.Count > 0)
        {
            await SeedPackDrawTableAsync(packId, DrawTier.Legendary, legendaryIds.ToArray());
        }
    }

    public async Task SeedPackDrawTableAsync(int packId, DrawTier tier, params long[] cardIds)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        if (await db.PackDrawConfigs.AnyAsync(c => c.Id == packId)) return;

        db.PackDrawConfigs.Add(new PackDrawConfigEntry { Id = packId, AnimationRatePct = 0 });
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.General, Tier = tier, RatePct = 100 });
        db.PackDrawSlotRates.Add(new PackDrawSlotRateEntry { PackId = packId, Slot = DrawSlot.Eighth,  Tier = tier, RatePct = 100 });
        foreach (var cid in cardIds)
        {
            db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry { PackId = packId, Slot = DrawSlot.General, Tier = tier, CardId = cid, RatePct = 100.0 / cardIds.Length });
            db.PackDrawCardWeights.Add(new PackDrawCardWeightEntry { PackId = packId, Slot = DrawSlot.Eighth,  Tier = tier, CardId = cid, RatePct = 100.0 / cardIds.Length });
        }
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Enables Freeplay mode by writing the GameConfigs DB row (tier-1 of GameConfigService).
    /// Call before issuing the request under test. Idempotent.
    /// </summary>
    public async Task EnableFreeplayAsync(ulong currencyAmount = 99999, int cardCopies = 3)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            Enabled = true,
            CurrencyAmount = currencyAmount,
            CardCopies = cardCopies,
        });
        var existing = await db.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == "Freeplay");
        if (existing is null)
            db.GameConfigs.Add(new GameConfigSection { SectionName = "Freeplay", ValueJson = json });
        else
            existing.ValueJson = json;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Enables SkipTutorial mode by writing the GameConfigs DB row. Fresh signups via
    /// <see cref="SVSim.Database.Repositories.Viewer.IViewerRepository.RegisterAnonymousViewer"/>
    /// will land at MissionData.TutorialState = 100 (post-tutorial) instead of 1. Idempotent.
    /// </summary>
    public async Task EnableSkipTutorialAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var json = System.Text.Json.JsonSerializer.Serialize(new { Enabled = true });
        var existing = await db.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == "SkipTutorial");
        if (existing is null)
            db.GameConfigs.Add(new GameConfigSection { SectionName = "SkipTutorial", ValueJson = json });
        else
            existing.ValueJson = json;
        await db.SaveChangesAsync();
    }

    /// <summary>Convenience: bake the X-Test-Viewer-Id header into a fresh client.</summary>
    public HttpClient CreateAuthenticatedClient(long viewerId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.ViewerIdHeader, viewerId.ToString());
        return client;
    }

    /// <summary>
    /// Shared secret baked into <c>appsettings.Testing.json</c> for <c>/admin/*</c> gating. Kept
    /// here so tests can construct requests with or without a valid header via the same source.
    /// </summary>
    public const string AdminSecret = "test-admin-secret";

    /// <summary>Convenience: bake the X-Admin-Secret header into a fresh client.</summary>
    public HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Secret", AdminSecret);
        return client;
    }

    /// <summary>
    /// Inserts a deck for the viewer via the real <see cref="IDeckRepository.UpsertDeck"/>
    /// path. Picks the first seeded class/sleeve/leader-skin from the master tables; tests
    /// that need specific ids should hit the DB directly.
    /// </summary>
    public async Task SeedDeckAsync(long viewerId, Format format, int number, string name = "Test Deck", int? classId = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IDeckRepository>();

        var cls = classId is null
            ? await db.Classes.FirstAsync()
            : await db.Classes.FindAsync(classId.Value)
                ?? throw new InvalidOperationException($"SeedDeckAsync: class {classId} not found");
        var sleeve = await db.Sleeves.FirstAsync();
        var skin = await db.LeaderSkins.FirstAsync();

        await repo.UpsertDeck(viewerId, format, number, d =>
        {
            d.Name = name;
            d.Class = cls;
            d.Sleeve = sleeve;
            d.LeaderSkin = skin;
        });
    }

    /// <summary>
    /// Seeds an OwnedCardEntry for the viewer. Uses an existing card from the minimal test set
    /// when <paramref name="cardId"/> matches one (10001001/10001002/10001003); otherwise the
    /// caller must have inserted the card row themselves. <paramref name="dustReward"/> is written
    /// onto the card's CollectionInfo so destruct tests can compute expected vials.
    ///
    /// NOTE: This helper ALWAYS resets the viewer's RedEther to 0 (so destruct tests can assert
    /// literal post-state totals). Callers that need a non-zero balance should re-assign after seeding.
    /// </summary>
    public async Task SeedOwnedCardAsync(
        long viewerId,
        long cardId,
        int count,
        int dustReward = 50,
        int craftCost = 200,
        bool isProtected = false)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
        if (card is null)
        {
            card = new ShadowverseCardEntry
            {
                Id = cardId,
                Name = $"SeededCard{cardId}",
                Rarity = Rarity.Bronze,
                CollectionInfo = new CardCollectionInfo { CraftCost = craftCost, DustReward = dustReward },
            };
            db.Cards.Add(card);
            await db.SaveChangesAsync();
        }
        else if (card.CollectionInfo is null || card.CollectionInfo.DustReward != dustReward || card.CollectionInfo.CraftCost != craftCost)
        {
            card.CollectionInfo = new CardCollectionInfo { CraftCost = craftCost, DustReward = dustReward };
            await db.SaveChangesAsync();
        }

        var viewer = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card).FirstAsync(v => v.Id == viewerId);
        var owned = viewer.Cards.FirstOrDefault(c => c.Card.Id == cardId);
        if (owned is null)
        {
            viewer.Cards.Add(new OwnedCardEntry { Card = card, Count = count, IsProtected = isProtected });
        }
        else
        {
            owned.Count = count;
            owned.IsProtected = isProtected;
        }
        viewer.Currency.RedEther = 0; // Reset RedEther so destruct tests can assert literal post-state totals
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a bare <see cref="ShadowverseCardEntry"/> (no viewer ownership) and returns its id.
    /// Used by InventoryGrantCardTests to get a valid card id without also seeding owned state.
    /// Ids start at 800_000_000 (non-foil) or 800_000_001 (foil) and increment by 2 per call to
    /// keep foil twins aligned.
    /// </summary>
    public async Task<long> SeedCardAsync(bool isFoil = false)
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        long id = isFoil ? 800_000_001L : 800_000_000L;
        while (await ctx.Cards.AnyAsync(c => c.Id == id)) id += 2;
        ctx.Cards.Add(new ShadowverseCardEntry { Id = id, IsFoil = isFoil, Name = $"SeedCard{id}" });
        await ctx.SaveChangesAsync();
        return id;
    }

    /// <summary>
    /// Sets the viewer's RedEther balance to <paramref name="amount"/>. Call this AFTER
    /// <see cref="SeedOwnedCardAsync"/>, which resets RedEther to 0. Create tests use this
    /// to give the viewer enough vials to craft.
    /// </summary>
    public async Task SetRedEtherAsync(long viewerId, ulong amount)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.Currency.RedEther = amount;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Puts <paramref name="count"/> copies of <paramref name="cardId"/> into the viewer's deck
    /// in the given format + slot. Tests use this to set up deck-strip scenarios for /card/destruct.
    /// The card must already exist (typically via SeedOwnedCardAsync, which inserts the card row).
    /// </summary>
    public async Task AddCardToDeckAsync(long viewerId, Format format, int deckNumber, long cardId, int count)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var viewer = await db.Viewers
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);

        var deck = viewer.Decks.First(d => d.Format == format && d.Number == deckNumber);
        var card = await db.Cards.FirstAsync(c => c.Id == cardId);

        var existing = deck.Cards.FirstOrDefault(c => c.Card.Id == cardId);
        if (existing is null)
        {
            deck.Cards.Add(new DeckCard { Card = card, Count = count });
        }
        else
        {
            existing.Count = count;
        }
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Reads the viewer's current <c>TutorialState</c> from the DB.
    /// Tests use this to verify that <c>/tutorial/update</c> persisted the step.
    /// </summary>
    public async Task<int> GetViewerTutorialStateAsync(long viewerId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.Include(v => v.MissionData).FirstAsync(v => v.Id == viewerId);
        return viewer.MissionData.TutorialState;
    }

    /// <summary>
    /// Reads the viewer's current currency balances from the DB. Used by gift_receive tests
    /// to assert delta grants after claiming tutorial presents.
    /// </summary>
    public async Task<(ulong Crystals, ulong Rupees, ulong RedEther)> GetViewerCurrencyAsync(long viewerId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        return (viewer.Currency.Crystals, viewer.Currency.Rupees, viewer.Currency.RedEther);
    }

    /// <summary>
    /// Seeds an OwnedItemEntry for the viewer. Inserts the ItemEntry master row if missing
    /// (Type defaults to 2 = card-pack ticket since both tutorial gift items 80001 and 90001
    /// are tickets). Tests use this to set up the ticket inventory that /tutorial/pack_open
    /// is supposed to consume.
    /// </summary>
    public async Task SeedOwnedItemAsync(long viewerId, int itemId, int count, string itemName = "TestItem", int itemType = 2)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var item = await db.Items.FindAsync(itemId);
        if (item is null)
        {
            item = new ItemEntry { Id = itemId, Name = itemName, Type = itemType };
            db.Items.Add(item);
            await db.SaveChangesAsync();
        }
        var viewer = await db.Viewers
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .FirstAsync(v => v.Id == viewerId);
        var existing = viewer.Items.FirstOrDefault(i => i.Item.Id == itemId);
        if (existing is null)
        {
            viewer.Items.Add(new OwnedItemEntry { Item = item, Count = count, Viewer = viewer });
        }
        else
        {
            existing.Count = count;
        }
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seed the tutorial ViewerPresent rows for a test viewer by projecting from the
    /// TutorialPresentEntries catalogue. RegisterViewer (admin/social path) does NOT auto-seed;
    /// only the production /tool/signup -> RegisterAnonymousViewer flow does. Tests opt in by
    /// calling this helper after SeedViewerAsync when they want a tutorial-shaped inbox state.
    /// If the catalogue is empty (most tests skip SeedGlobalsAsync), this method imports
    /// tutorial-presents.json on demand so the helper works regardless of test setup ordering.
    /// </summary>
    public async Task SeedTutorialPresentsAsync(long viewerId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        if (!await db.TutorialPresentEntries.AnyAsync())
        {
            string seedDir = Path.Combine(AppContext.BaseDirectory, "Data", "seeds");
            await new TutorialPresentsImporter().ImportAsync(db, seedDir);
        }

        var catalogue = await db.TutorialPresentEntries
            .AsNoTracking()
            .OrderBy(p => p.PresentId)
            .ToListAsync();

        var createdAt = DateTime.UtcNow;
        foreach (var spec in catalogue)
        {
            db.ViewerPresents.Add(new ViewerPresent
            {
                ViewerId       = viewerId,
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
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Reads the viewer's current owned count for <paramref name="itemId"/>. Returns 0 if no
    /// row exists. Tests use this to assert ticket consumption after /tutorial/pack_open.
    /// </summary>
    public async Task<int> GetOwnedItemCountAsync(long viewerId, int itemId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .FirstAsync(v => v.Id == viewerId);
        return viewer.Items.FirstOrDefault(i => i.Item.Id == itemId)?.Count ?? 0;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
