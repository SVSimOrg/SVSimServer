using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SVSim.Bootstrap.Importers;
using SVSim.Database;

namespace SVSim.Bootstrap;

public static class Program
{
    private const string DefaultConnectionString =
        "Host=localhost;Database=svsim;Username=postgres;password=postgres";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && (args[0] is "--help" or "-h"))
        {
            PrintUsage();
            return 1;
        }

        var opts = ParseArgs(args);
        if (opts is null)
        {
            PrintUsage();
            return 1;
        }

        if (opts.SkipReference && opts.SkipCards && opts.SkipGlobals && opts.SkipStory)
        {
            Console.Error.WriteLine("All --skip-* flags set; nothing to do.");
            return 1;
        }

        Console.WriteLine($"[Bootstrap] Connection:    {RedactPassword(opts.ConnectionString)}");
        Console.WriteLine($"[Bootstrap] Reference CSVs: {opts.ReferenceDataDir}");
        Console.WriteLine($"[Bootstrap] Cards file:    {opts.CardsFile}");
        Console.WriteLine($"[Bootstrap] Seeds:         {opts.SeedDir}");

        var dbOptions = new DbContextOptionsBuilder<SVSimDbContext>()
            .UseNpgsql(opts.ConnectionString)
            .Options;

        await using var context = new SVSimDbContext(NullLogger<SVSimDbContext>.Instance, dbOptions);

        // Bootstrap applies pending migrations first — migrations are now DDL-only, all data
        // (reference tables, cards, card cosmetic rewards, per-table seed globals, game config)
        // is loaded by importers below. This means a freshly migrated DB is structure-only;
        // every importer is idempotent so re-running is safe.
        Console.WriteLine("[Bootstrap] Applying pending migrations...");
        await context.Database.MigrateAsync();

        // GameConfigSection rows for every [ConfigSection] type — runtime seed (HasData doesn't
        // play well with OwnsOne+ToJson). Always run; tiers only insert missing sections.
        await context.EnsureSeedDataAsync();

        if (!opts.SkipReference)
        {
            await new ReferenceDataImporter().ImportAllAsync(context, opts.ReferenceDataDir);
        }
        else
        {
            Console.WriteLine("[Bootstrap] --skip-reference set; skipping reference data import.");
        }

        if (!opts.SkipCards)
        {
            await new CardImporter().ImportAsync(context, opts.CardsFile);
            // Card cosmetic rewards FK to Cards; piggy-back on --skip-cards.
            await new CardCosmeticRewardImporter().ImportAsync(context, opts.ReferenceDataDir);
        }
        else
        {
            Console.WriteLine("[Bootstrap] --skip-cards set; skipping card + cosmetic-reward import.");
        }

        if (!opts.SkipGlobals)
        {
            // Per-domain seed pipeline. Each importer reads a per-table JSON seed file under
            // SVSim.Bootstrap/Data/seeds/ produced by an extractor in data_dumps/scripts/.
            //
            // RotationConfigImporter writes the Rotation GameConfig section that RotationFlagUpdater
            // reads; CardImporter ran earlier in the !SkipCards block so CardSets are populated.
            await new RotationConfigImporter().ImportAsync(context, opts.SeedDir);
            await new MyRotationImporter().ImportAsync(context, opts.SeedDir);
            await new AvatarAbilityImporter().ImportAsync(context, opts.SeedDir);
            await new ArenaSeasonImporter().ImportAsync(context, opts.SeedDir);
            await new ArenaTwoPickRewardImporter().ImportAsync(context, opts.SeedDir);
            await new ColosseumHofDecksImporter().ImportAsync(context, opts.SeedDir);
            await new ColosseumWindFallDecksImporter().ImportAsync(context, opts.SeedDir);
            await new ColosseumAvatarDecksImporter().ImportAsync(context, opts.SeedDir);
            await new BattlePassImporter().ImportAsync(context, opts.SeedDir);
            await new BattlePassSeasonImporter().ImportAsync(context, opts.SeedDir);
            await new BattlePassRewardImporter().ImportAsync(context, opts.SeedDir);
            await new MissionCatalogImporter().ImportAsync(context, opts.SeedDir);
            await new AchievementCatalogImporter().ImportAsync(context, opts.SeedDir);
            await new BattlePassMonthlyMissionImporter().ImportAsync(context, opts.SeedDir);
            await new PreReleaseInfoImporter().ImportAsync(context, opts.SeedDir);
            await new CardListsImporter().ImportAsync(context, opts.SeedDir);
            await new RotationFlagUpdater().UpdateAsync(context);

            await new PracticeOpponentImporter().ImportAsync(context, opts.SeedDir);
            await new BotRosterImporter().ImportAsync(context, opts.SeedDir);
            await new PaymentItemImporter().ImportAsync(context, opts.SeedDir);
            await new ItemImporter().ImportAsync(context, opts.SeedDir);
            await new SleeveShopImporter().ImportAsync(context, opts.SeedDir);
            await new ItemPurchaseImporter().ImportAsync(context, opts.SeedDir);
            await new LeaderSkinShopImporter().ImportAsync(context, opts.SeedDir);
            await new SpotCardExchangeImporter().ImportAsync(context, opts.SeedDir);
            var puzzleImporter = new PuzzleImporter();
            await puzzleImporter.ImportGroupsAsync(context, opts.SeedDir);
            await puzzleImporter.ImportPuzzlesAsync(context, opts.SeedDir);
            await puzzleImporter.ImportMissionsAsync(context, opts.SeedDir);

            var mypage = new MyPageGlobalsImporter();
            await mypage.ImportBannersAsync(context, opts.SeedDir);
            await mypage.ImportSealedAsync(context, opts.SeedDir);
            await mypage.ImportMasterPointRankingPeriodAsync(context, opts.SeedDir);
            await mypage.ImportSpecialDeckFormatsAsync(context, opts.SeedDir);
            await mypage.ImportHomeDialogsAsync(context, opts.SeedDir);

            await new TutorialPresentsImporter().ImportAsync(context, opts.SeedDir);

            await new DefaultDeckImporter().ImportAsync(context, opts.SeedDir);
            await new PackImporter().ImportAsync(context, opts.SeedDir);
            await new PackDrawTableImporter().ImportAsync(context, opts.SeedDir);

            // BuildDeck pipeline: series CSV → catalog JSON → package CSV. Catalog must run after
            // series CSV (FK on products → series) and before package CSV (so the catalog-side
            // enriched rows take precedence over stub creation).
            var buildDeck = new BuildDeckImporter();
            await buildDeck.ImportSeriesAsync(context, opts.ReferenceDataDir);
            await buildDeck.ImportCatalogAsync(context, opts.SeedDir);
            await buildDeck.ImportPackageAsync(context, opts.ReferenceDataDir);
            await new StoryDeckImporter().ImportAsync(context, opts.SeedDir);
        }
        else
        {
            Console.WriteLine("[Bootstrap] --skip-globals set; skipping globals import.");
        }

        if (!opts.SkipStory)
        {
            await new StoryImporter().ImportAsync(context, opts.StoryDataDir);
        }
        else
        {
            Console.WriteLine("[Bootstrap] --skip-story set; skipping story import.");
        }

        Console.WriteLine("[Bootstrap] Complete.");
        return 0;
    }

    private static BootstrapOptions? ParseArgs(string[] args)
    {
        string? cards = null;
        string? referenceDataDir = null;
        string? connection = null;
        bool skipReference = false;
        bool skipCards = false;
        bool skipGlobals = false;
        bool skipStory = false;
        string? storyDataDir = null;
        string? positionalCards = null;

        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            switch (a)
            {
                case "--cards": cards = NextArg(args, ref i); break;
                case "--reference-data-dir": referenceDataDir = NextArg(args, ref i); break;
                case "--connection-string": connection = NextArg(args, ref i); break;
                case "--skip-reference": skipReference = true; break;
                case "--skip-cards": skipCards = true; break;
                case "--skip-globals": skipGlobals = true; break;
                case "--skip-story": skipStory = true; break;
                case "--story-data-dir": storyDataDir = NextArg(args, ref i); break;
                default:
                    // Back-compat: legacy positional form `svsim-card-import <cards.json> [connection]`.
                    if (positionalCards is null && !a.StartsWith('-')) positionalCards = a;
                    else if (connection is null && !a.StartsWith('-')) connection = a;
                    else { Console.Error.WriteLine($"Unknown argument: {a}"); return null; }
                    break;
            }
        }

        // All bootstrap inputs ship in-project under SVSim.Bootstrap/Data/, copied next to the
        // binary on build. The --cards/--reference-data-dir flags are ad-hoc overrides
        // (e.g. point at a fresh loader dump before promoting it into the project).
        string baseDir = AppContext.BaseDirectory;
        string shippedDataDir = Path.Combine(baseDir, "Data");
        string shippedCardsFile = Path.Combine(shippedDataDir, "cards.json");

        string cardsFile = cards ?? positionalCards ?? shippedCardsFile;
        string refDir = referenceDataDir ?? shippedDataDir;
        string shippedStoryDir = Path.Combine(shippedDataDir, "story");
        string storyDir = storyDataDir ?? shippedStoryDir;
        string shippedSeedDir = Path.Combine(shippedDataDir, "seeds");

        string connStr = connection
            ?? Environment.GetEnvironmentVariable("NPGSQL_CONNECTION")
            ?? DefaultConnectionString;

        return new BootstrapOptions(
            cardsFile, refDir, connStr, skipReference, skipCards, skipGlobals,
            skipStory, storyDir, shippedSeedDir);
    }

    private static string NextArg(string[] args, ref int i)
    {
        if (i + 1 >= args.Length) throw new ArgumentException($"Missing value for {args[i]}");
        return args[++i];
    }

    private static string RedactPassword(string conn) =>
        System.Text.RegularExpressions.Regex.Replace(conn, "(?i)(password=)[^;]+", "$1***");

    private static void PrintUsage()
    {
        Console.Error.WriteLine(
            "Usage: svsim-bootstrap [options]\n" +
            "\n" +
            "  All inputs default to the in-project SVSim.Bootstrap/Data/ folder, copied next to\n" +
            "  the binary at build time. Override flags below take ad-hoc paths (e.g. a fresh\n" +
            "  loader dump) — promote into Data/ when you're ready to make it permanent.\n" +
            "\n" +
            "  --cards <file>                Override path to cards.json (default: shipped Data/cards.json)\n" +
            "  --reference-data-dir <dir>    Override reference CSV directory (default: shipped Data/)\n" +
            "  --connection-string <conn>    Postgres connection (or NPGSQL_CONNECTION env var,\n" +
            $"                                then \"{DefaultConnectionString}\")\n" +
            "  --skip-reference              Skip reference-data import (classes, sleeves, ranks, ...)\n" +
            "  --skip-cards                  Skip card + card-cosmetic-reward import\n" +
            "  --skip-globals                Skip seed-driven globals import (per-table JSON under Data/seeds)\n" +
            "  --story-data-dir <dir>        Override story data directory (default: shipped Data/story)\n" +
            "  --skip-story                  Skip story import (worlds/sections/chapters/sbs)\n" +
            "\n" +
            "Capture-derived seeds are produced by extractors under data_dumps/scripts/* and\n" +
            "checked into SVSim.Bootstrap/Data/seeds/. The bootstrap project never parses wire\n" +
            "captures directly — refresh seeds by re-running the relevant extractor.\n" +
            "\n" +
            "Back-compat: `svsim-bootstrap <cards.json> [connection]` still works (positional).");
    }

    private sealed record BootstrapOptions(
        string CardsFile,
        string ReferenceDataDir,
        string ConnectionString,
        bool SkipReference,
        bool SkipCards,
        bool SkipGlobals,
        bool SkipStory,
        string StoryDataDir,
        string SeedDir);
}
