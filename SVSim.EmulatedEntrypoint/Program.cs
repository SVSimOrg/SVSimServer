using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Repositories.BuildDeck;
using SVSim.Database.Repositories.Card;
using SVSim.Database.Repositories.Collectibles;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Pack;
using SVSim.Database.Repositories.Story;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.Friend;
using SVSim.Database.Services.Replay;
using SVSim.EmulatedEntrypoint.Configuration;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Infrastructure;
using SVSim.EmulatedEntrypoint.Matching;
using SVSim.EmulatedEntrypoint.Middlewares;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.BattleNode.Hosting;
using Serilog;
using SVSim.Hosting;

namespace SVSim.EmulatedEntrypoint;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Always register Serilog. The file sink self-gates on the Testing environment inside
        // the UseSvSimSerilog callback — a caller-side check here would be too early because
        // WebApplicationFactory applies UseEnvironment("Testing") after CreateBuilder returns.
        builder.Host.UseSvSimSerilog("svsim-api");

        try
        {
        // Add services to the container.

        builder.Services.AddControllers().AddJsonOptions(opt =>
        {
            // Wire-format congruence: the encrypted msgpack path uses snake_case [Key("...")]
            // names; the plain-JSON path runs through System.Text.Json. Match them by using
            // SnakeCaseLower naming policy here so both paths emit identical key names — and
            // so the translation middleware can hand JSON keys straight through to msgpack
            // without per-property name remapping.
            opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            // Production omits null/optional fields entirely; the client uses
            // `Keys.Contains(name)` as a presence check and calls `.ToInt()` (etc.) on the
            // value without a null guard. Emitting `"key":null` makes Contains return true and
            // crashes the client. Drop nulls during serialization so missing == absent.
            opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            // Format-typed properties serialize to/from the wire deck_format int via the
            // client's FormatConvertApi mapping. See FormatExtensions.cs.
            opt.JsonSerializerOptions.Converters.Add(new FormatJsonConverter());
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            // Disambiguate same-named DTOs across families (e.g. Story.StartRequest vs
            // BasicPuzzle.StartRequest) by qualifying schema ids with the full type name.
            c.CustomSchemaIds(t => t.FullName?.Replace("+", "."));

            // Register the X-Admin-Secret shared-secret header so the Swagger UI shows an
            // Authorize dialog for it. AdminSecretOperationFilter then attaches the requirement
            // only to endpoints that carry [RequireAdminSecret], keeping the padlock off
            // ordinary game endpoints.
            c.AddSecurityDefinition(AdminSecretOperationFilter.SchemeId, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = RequireAdminSecretAttribute.HeaderName,
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Description = "Shared secret for /admin/* endpoints (matches Admin:ImportSecret in appsettings).",
            });
            c.OperationFilter<AdminSecretOperationFilter>();
        });
        builder.Services.AddHttpLogging(opt =>
        {

        });

        builder.Services.Configure<DeckOptions>(builder.Configuration.GetSection(DeckOptions.SectionName));
        builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));

        #region Database Services

        builder.Services.AddDbContext<SVSimDbContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("ApplicationDb"));
        });
        builder.Services.AddTransient<IViewerRepository, ViewerRepository>();
        builder.Services.AddTransient<IPuzzleClearRepository, PuzzleClearRepository>();
        builder.Services.AddTransient<ICardRepository, CardRepository>();
        builder.Services.AddTransient<ICardInventoryRepository, CardInventoryRepository>();
        builder.Services.AddTransient<ICollectionRepository, CollectionRepository>();
        builder.Services.AddTransient<IGlobalsRepository, GlobalsRepository>();
        builder.Services.AddTransient<IPuzzleCatalogRepository, PuzzleCatalogRepository>();
        builder.Services.AddTransient<IArenaTwoPickRewardRepository, ArenaTwoPickRewardRepository>();
        builder.Services.AddTransient<IDeckRepository, DeckRepository>();
        builder.Services.AddTransient<IPackRepository, PackRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.PackDrawTables.IPackDrawTableRepository, SVSim.Database.Repositories.PackDrawTables.PackDrawTableRepository>();
        builder.Services.AddTransient<IBuildDeckRepository, BuildDeckRepository>();
        // Scoped (not Singleton) to avoid the singleton-depends-on-scoped-DbContext lifecycle
        // pitfall. Cost: one indexed single-row query per section per request — trivial. No
        // in-process cache today; the IGameConfigService interface is shaped to allow one later.
        builder.Services.AddScoped<SVSim.Database.Services.IGameConfigService, GameConfigService>();
        builder.Services.AddScoped<ICardFoilLookup, DbCardFoilLookup>();
        builder.Services.AddScoped<PackOpenService>();
        builder.Services.AddScoped<IGachaPointService, GachaPointService>();
        builder.Services.AddScoped<SVSim.Database.Services.Inventory.IInventoryService,
                                  SVSim.Database.Services.Inventory.InventoryService>();
        builder.Services.AddScoped<SVSim.Database.Services.BattleXp.IBattleXpService,
                                  SVSim.Database.Services.BattleXp.BattleXpService>();
        builder.Services.AddScoped<SVSim.Database.Services.RankProgress.IRankProgressService,
                                  SVSim.Database.Services.RankProgress.RankProgressService>();
        builder.Services.AddScoped<SVSim.Database.Repositories.BattlePass.IBattlePassRepository,
                                    SVSim.Database.Repositories.BattlePass.BattlePassRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.BattlePass.IViewerBattlePassRepository,
                                    SVSim.Database.Repositories.BattlePass.ViewerBattlePassRepository>();
        builder.Services.AddScoped<IBattlePassService, BattlePassService>();
        builder.Services.AddScoped<SVSim.Database.Repositories.Mission.IMissionCatalogRepository,
                                    SVSim.Database.Repositories.Mission.MissionCatalogRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.Mission.IViewerMissionRepository,
                                    SVSim.Database.Repositories.Mission.ViewerMissionRepository>();
        builder.Services.AddScoped<IMissionProgressService, MissionProgressService>();
        builder.Services.AddScoped<IViewerMissionStateService, ViewerMissionStateService>();
        builder.Services.AddScoped<IMissionAssembler, MissionAssembler>();
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
        builder.Services.AddScoped<IStoryMasterRepository, StoryMasterRepository>();
        builder.Services.AddScoped<IViewerStoryProgressRepository, ViewerStoryProgressRepository>();
        builder.Services.AddScoped<IArenaTwoPickRunRepository, ArenaTwoPickRunRepository>();
        builder.Services.AddScoped<IArenaColosseumRunRepository, ArenaColosseumRunRepository>();
        builder.Services.AddScoped<SVSim.EmulatedEntrypoint.Services.ArenaColosseum.IColosseumProgressionService,
                                   SVSim.EmulatedEntrypoint.Services.ArenaColosseum.ColosseumProgressionService>();
        builder.Services.AddScoped<IArenaTwoPickCardPoolService, ArenaTwoPickCardPoolService>();
        builder.Services.AddScoped<IArenaTwoPickService, ArenaTwoPickService>();
        builder.Services.AddScoped<IMatchContextBuilder, MatchContextBuilder>();
        builder.Services.AddScoped<IStoryService, StoryService>();
        builder.Services.AddScoped<ILoginBonusService, LoginBonusService>();
        builder.Services.AddScoped<IGameCalendarService, GameCalendarService>();
        builder.Services.AddScoped<IDeckListBuilder, DeckListBuilder>();
        builder.Services.AddSingleton<IRandom, SystemRandom>();
        builder.Services.AddSingleton<PuzzleMissionEvaluator>();

        builder.Services.AddScoped<IFriendService, FriendService>();
        builder.Services.AddScoped<IPlayedTogetherWriter, FriendService>();

        builder.Services.AddScoped<SVSim.Database.Repositories.Guild.IGuildRepository, SVSim.Database.Repositories.Guild.GuildRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.Guild.IGuildMemberRepository, SVSim.Database.Repositories.Guild.GuildMemberRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.Guild.IGuildInviteRepository, SVSim.Database.Repositories.Guild.GuildInviteRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.Guild.IGuildJoinRequestRepository, SVSim.Database.Repositories.Guild.GuildJoinRequestRepository>();
        builder.Services.AddScoped<SVSim.Database.Repositories.Guild.IGuildChatMessageRepository, SVSim.Database.Repositories.Guild.GuildChatMessageRepository>();
        builder.Services.AddSingleton<SVSim.Database.Services.Guild.IGuildIdGenerator, SVSim.Database.Services.Guild.GuildIdGenerator>();
        builder.Services.AddScoped<SVSim.Database.Services.Guild.IGuildService, SVSim.Database.Services.Guild.GuildService>();
        builder.Services.AddScoped<SVSim.Database.Services.Guild.IGuildChatService, SVSim.Database.Services.Guild.GuildChatService>();

        builder.Services.AddSingleton<IBattleContextStore, InMemoryBattleContextStore>();
        builder.Services.AddScoped<IBattleHistoryWriter, BattleHistoryWriter>();
        builder.Services.AddScoped<IReplayHistoryReader, ReplayHistoryReader>();

        // Deck-code mint/resolve uses IMemoryCache for ephemeral (3-min TTL) storage; no DB
        // row, no migration. Singleton because the cache + RNG seam are process-wide.
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IDeckCodeService, DeckCodeService>();

        // Per-process per-viewer tracker for home_dialog_list suppression on /mypage/index.
        // Restart re-fires once per viewer — documented trade in the design spec.
        builder.Services.AddSingleton<IHomeDialogSessionTracker, HomeDialogSessionTracker>();

        // Loads the static card-master base64 blob from Data/ once at startup; served by
        // ImmutableDataController. Singleton because the file is ~1.27 MB.
        builder.Services.AddSingleton<ICardMasterPayloadProvider, CardMasterPayloadProvider>();

        #endregion

        builder.Services.AddBattleNode(opt =>
        {
            // Every field on BattleNodeOptions is populated from the "BattleNode" section in
            // appsettings*.json. NodeServerUrl has no hardcoded fallback — AddBattleNode
            // throws at startup if it's missing/blank. See BattleNodeOptions.NodeServerUrl
            // for the required wire format.
            builder.Configuration.GetSection("BattleNode").Bind(opt);
        });
        // In-process FCFS pair-up for TK2 PvP /do_matching, plus rank-battle's AI-fallback
        // branch. Singleton: per-mode state is process-wide. Proper queue API is a separate
        // spec; this is enough to actually pair two viewers polling the same mode end-to-end.
        builder.Services.AddSingleton(new ModePolicyRegistry(new[]
        {
            new ModePolicy("arena_two_pick_battle", PolicyKind.PvpOnly),
            new ModePolicy("rotation_rank_battle", PolicyKind.PvpFirstThenAiFallback),
            new ModePolicy("unlimited_rank_battle", PolicyKind.PvpFirstThenAiFallback),
            // Free battle is casual PvP — no /ai_*_free_battle URL family exists in the client
            // (see Shadowverse_Code_2026-05-23/ApiType.cs), so PvpOnly: park forever, no AI fallback.
            new ModePolicy("rotation_free_battle", PolicyKind.PvpOnly),
            new ModePolicy("unlimited_free_battle", PolicyKind.PvpOnly),
            // Colosseum (Grand Prix). Pre-promotion + post-promotion (rank) URLs each map
            // to their own pair-up mode — the URL IS the signal per do-matching.md, and the
            // node session has no way to ask the client "which bracket are you in?".
            new ModePolicy("colosseum_battle", PolicyKind.PvpOnly),
            new ModePolicy("colosseum_rank_battle", PolicyKind.PvpOnly),
        }));
        builder.Services.AddSingleton<IMatchingPairUpService, InProcessPairUp>();
        // Single resolver shared by every /do_matching family controller. Owns the
        // pair-up → matching_state mapping. Singleton: stateless, all deps are singletons too.
        builder.Services.AddSingleton<IMatchingResolver, MatchingResolver>();
        // Phase 3: bot roster used by RankBattleController.AiStart to compose oppo_info.
        // Transient because BotRoster depends on the transient IGlobalsRepository.
        builder.Services.AddTransient<IBotRoster, BotRoster>();

        builder.Services.AddTransient<ShadowverseTranslationMiddleware>();
        builder.Services.AddTransient<SessionidMappingMiddleware>();
        builder.Services.AddSingleton<ShadowverseSessionService>();
        // Steam ticket validation seam. Production uses Facepunch against real Steam. Local dev
        // can opt into a no-op validator via Auth:BypassSteamTicket so clients without a real
        // Steam session (e.g. a second same-machine instance for the two-client PvP smoke) can
        // authenticate. Gate is config-only and ships false everywhere except Development.
        if (builder.Configuration.GetValue<bool>("Auth:BypassSteamTicket"))
        {
            builder.Services.AddSingleton<ISteamServer, DevAlwaysValidSteamServer>();
        }
        else
        {
            builder.Services.AddSingleton<ISteamServer, FacepunchSteamServer>();
        }
        builder.Services.AddSingleton<SteamSessionService>();
        builder.Services.AddAuthentication()
            .AddScheme<SteamAuthenticationHandlerOptions, SteamSessionAuthenticationHandler>(
                SteamAuthenticationConstants.SchemeName,
                opt =>
                {
                    
                });

        var app = builder.Build();

        // Update database (skipped for non-relational providers, e.g. InMemory in tests, and
        // skipped under the "Testing" environment where the test fixture has already called
        // EnsureCreated against a SQLite in-memory DB — the Postgres migrations would fail there).
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            if (dbContext.Database.IsRelational() && !app.Environment.IsEnvironment("Testing"))
            {
                dbContext.UpdateDatabase();
                dbContext.EnsureSeedDataAsync().GetAwaiter().GetResult();
            }
        }

        // HttpLogging captures full request/response per call. In Testing it pipes ~3 GB of
        // stdout into NUnit's per-test result capture across the suite, which OOMs the trx
        // serializer. Production keeps it on.
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpLogging();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        app.UseMiddleware<SessionidMappingMiddleware>();
        
        app.UseMiddleware<ShadowverseTranslationMiddleware>();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseBattleNode();

        app.MapControllers();

        app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
