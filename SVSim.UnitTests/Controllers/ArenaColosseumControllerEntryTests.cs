using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Phase 1 entry + register-deck coverage. Activating the Colosseum season requires writing
/// a <c>ColosseumSeason</c> + <c>ColosseumRounds</c> row to <c>GameConfigs</c> — see
/// <see cref="ActivateSeasonAsync"/> for the test-only equivalent of the admin flow.
/// </summary>
public class ArenaColosseumControllerEntryTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    private static async Task ActivateSeasonAsync(SVSimTestFactory factory, int crystalCost = 300)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var seasonJson = JsonSerializer.Serialize(new
        {
            IsColosseumPeriod = true,
            SeasonId = 42,
            ColosseumName = "Test Cup",
            DeckFormat = (int)Format.Rotation,
            CrystalCost = crystalCost,
            RupyCost = 3000,
            TicketCost = 1,
            IsAllowedFreeEntry = false,
        });
        await UpsertConfigAsync(db, "ColosseumSeason", seasonJson);

        var roundsJson = JsonSerializer.Serialize(new
        {
            Rounds = new[]
            {
                new
                {
                    RoundId = 1,
                    StartTime = DateTime.UtcNow.AddDays(-1),
                    EndTime = DateTime.UtcNow.AddDays(7),
                    Groups = new[]
                    {
                        new { Group = "", MaxBattleCount = 5, BreakthroughNumber = 4, EntryNumber = 100_000 },
                    },
                },
            },
        });
        await UpsertConfigAsync(db, "ColosseumRounds", roundsJson);
    }

    private static async Task UpsertConfigAsync(SVSimDbContext db, string section, string json)
    {
        var existing = await db.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == section);
        if (existing is null)
            db.GameConfigs.Add(new GameConfigSection { SectionName = section, ValueJson = json });
        else
            existing.ValueJson = json;
        await db.SaveChangesAsync();
    }

    private static async Task SetViewerCurrencyAsync(SVSimTestFactory factory, long viewerId, ulong crystals = 0, ulong rupees = 0)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.Currency.Crystals = crystals;
        viewer.Currency.Rupees = rupees;
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Entry_debits_crystal_and_creates_run()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonAsync(factory, crystalCost: 300);
        var viewerId = await factory.SeedViewerAsync();
        await SetViewerCurrencyAsync(factory, viewerId, crystals: 1000);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/arena_colosseum/entry",
            JsonContent.Create(new { consume_item_type = 1, now_round_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(1));
        Assert.That(root.GetProperty("reward_list")[0].GetProperty("reward_type").GetInt32(), Is.EqualTo((int)UserGoodsType.Crystal));
        Assert.That(root.GetProperty("entry_info").GetProperty("deck_format").GetInt32(), Is.EqualTo((int)Format.Rotation));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstOrDefaultAsync(r => r.ViewerId == viewerId);
        Assert.That(run, Is.Not.Null);
        Assert.That(run!.SeasonId, Is.EqualTo(42));
        Assert.That(run.MaxBattleCountThisRound, Is.EqualTo(5));
        Assert.That(run.BreakthroughNumberThisRound, Is.EqualTo(4));

        var viewerAfter = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewerAfter.Currency.Crystals, Is.EqualTo(700UL), "1000 - 300 cost");
    }

    [Test]
    public async Task Entry_rejects_when_season_inactive()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/arena_colosseum/entry",
            JsonContent.Create(new { consume_item_type = 1, now_round_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("colosseum_period_closed", body);
    }

    [Test]
    public async Task Entry_rejects_when_already_in_run()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonAsync(factory);
        var viewerId = await factory.SeedViewerAsync();
        await SetViewerCurrencyAsync(factory, viewerId, crystals: 1000);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
            {
                ViewerId = viewerId,
                EntryId = 999,
                SeasonId = 42,
                RoundId = 1,
                DeckFormat = Format.Rotation,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/arena_colosseum/entry",
            JsonContent.Create(new { consume_item_type = 1, now_round_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("arena_colosseum_already_in_progress", body);
    }

    [Test]
    public async Task Entry_rejects_when_now_round_id_mismatch()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonAsync(factory);
        var viewerId = await factory.SeedViewerAsync();
        await SetViewerCurrencyAsync(factory, viewerId, crystals: 1000);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/arena_colosseum/entry",
            JsonContent.Create(new { consume_item_type = 1, now_round_id = 7, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("now_round_id_mismatch", body);
    }

    [Test]
    public async Task RegisterDeck_round_trips_deck_no_list()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonAsync(factory);
        var viewerId = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(viewerId, Format.Rotation, number: 3, name: "Colo Deck 3");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
            {
                ViewerId = viewerId,
                EntryId = 999,
                SeasonId = 42,
                RoundId = 1,
                DeckFormat = Format.Rotation,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/arena_colosseum/register_deck",
            JsonContent.Create(new { deck_no_list = "[3]", is_published = true, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await verifyDb.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == viewerId);
        Assert.That(run.RegisteredDeckNoListJson, Is.EqualTo("[3]"));
        Assert.That(run.IsPublished, Is.True);
    }

    [Test]
    public async Task RegisterDeck_rejects_when_deck_not_found()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonAsync(factory);
        var viewerId = await factory.SeedViewerAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
            {
                ViewerId = viewerId,
                EntryId = 999,
                SeasonId = 42,
                RoundId = 1,
                DeckFormat = Format.Rotation,
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/arena_colosseum/register_deck",
            JsonContent.Create(new { deck_no_list = "[99]", is_published = false, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("deck_not_found", body);
    }
}
