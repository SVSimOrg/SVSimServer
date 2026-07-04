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
/// Phase 3 curated-deck coverage — the 3 list URLs and 3 register URLs share one
/// generic dispatcher; the same scenarios are parameterized across HOF / WindFall / Avatar
/// to lock per-pool isolation.
/// </summary>
public class ArenaColosseumControllerCuratedDeckTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    public enum Pool { Hof, WindFall, Avatar }

    private static string ListUrl(Pool pool) => pool switch
    {
        Pool.Hof => "/arena_colosseum/get_hof_deck_list",
        Pool.WindFall => "/arena_colosseum/get_windfall_deck_list",
        Pool.Avatar => "/arena_colosseum/get_avatar_deck_list",
        _ => throw new ArgumentOutOfRangeException(),
    };

    private static string RegisterUrl(Pool pool) => pool switch
    {
        Pool.Hof => "/arena_colosseum/register_hof_deck",
        Pool.WindFall => "/arena_colosseum/register_windfall_deck",
        Pool.Avatar => "/arena_colosseum/register_avatar_deck",
        _ => throw new ArgumentOutOfRangeException(),
    };

    private static async Task SeedCuratedDecksAsync(SVSimTestFactory factory, Pool pool, params int[] deckNos)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        foreach (var no in deckNos)
        {
            switch (pool)
            {
                case Pool.Hof:
                    db.ColosseumHofDecks.Add(new ColosseumHofDeck
                    {
                        DeckNo = no, ClassId = 1, DisplayOrder = no,
                        CardListJson = "[101,102,103]", SleeveId = 3000011,
                    });
                    break;
                case Pool.WindFall:
                    db.ColosseumWindFallDecks.Add(new ColosseumWindFallDeck
                    {
                        DeckNo = no, ClassId = 2, DisplayOrder = no,
                        CardListJson = "[201,202,203]",
                    });
                    break;
                case Pool.Avatar:
                    db.ColosseumAvatarDecks.Add(new ColosseumAvatarDeck
                    {
                        DeckNo = no, ClassId = 3, DisplayOrder = no,
                        CardListJson = "[301,302,303]", LeaderSkinId = 70000,
                    });
                    break;
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedRunAsync(SVSimTestFactory factory, long viewerId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
        {
            ViewerId = viewerId,
            EntryId = 9999,
            SeasonId = 7,
            RoundId = 1,
            DeckFormat = Format.Hof,
        });
        await db.SaveChangesAsync();
    }

    [Test]
    [TestCase(Pool.Hof)]
    [TestCase(Pool.WindFall)]
    [TestCase(Pool.Avatar)]
    public async Task GetDeckList_returns_seeded_entries_as_bare_array(Pool pool)
    {
        using var factory = new SVSimTestFactory();
        await SeedCuratedDecksAsync(factory, pool, 1001, 1002);
        var vid = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(vid);

        var resp = await client.PostAsync(ListUrl(pool), JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array),
            $"{pool}: spec requires a BARE array at data — client iterates without a wrapper");
        Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(2));
        Assert.That(doc.RootElement[0].GetProperty("deck_id").GetInt32(), Is.EqualTo(1001));
    }

    [Test]
    [TestCase(Pool.Hof)]
    [TestCase(Pool.WindFall)]
    [TestCase(Pool.Avatar)]
    public async Task RegisterDeck_round_trips_deck_no_list(Pool pool)
    {
        using var factory = new SVSimTestFactory();
        await SeedCuratedDecksAsync(factory, pool, 1001);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        var resp = await client.PostAsync(RegisterUrl(pool),
            JsonContent.Create(new { deck_no_list = "[1001]", viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(run.RegisteredDeckNoListJson, Is.EqualTo("[1001]"));
    }

    [Test]
    [TestCase(Pool.Hof)]
    [TestCase(Pool.WindFall)]
    [TestCase(Pool.Avatar)]
    public async Task RegisterDeck_rejects_unknown_deck_no(Pool pool)
    {
        using var factory = new SVSimTestFactory();
        await SeedCuratedDecksAsync(factory, pool, 1001);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        var resp = await client.PostAsync(RegisterUrl(pool),
            JsonContent.Create(new { deck_no_list = "[9999]", viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("deck_not_found", body);
    }

    [Test]
    public async Task Cross_pool_register_rejected_hof_against_windfall()
    {
        using var factory = new SVSimTestFactory();
        // 1001 lives in HOF only.
        await SeedCuratedDecksAsync(factory, Pool.Hof, 1001);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);
        using var client = factory.CreateAuthenticatedClient(vid);

        // Register against WindFall — the HOF deck_no should not resolve.
        var resp = await client.PostAsync(RegisterUrl(Pool.WindFall),
            JsonContent.Create(new { deck_no_list = "[1001]", viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("deck_not_found", body);
    }

    [Test]
    public async Task RegisterDeck_clears_is_published_flag_on_swap_from_constructed()
    {
        using var factory = new SVSimTestFactory();
        await SeedCuratedDecksAsync(factory, Pool.Hof, 1001);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid);

        // Simulate the viewer having previously registered a constructed deck with is_published=true.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
            run.IsPublished = true;
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        await client.PostAsync(RegisterUrl(Pool.Hof),
            JsonContent.Create(new { deck_no_list = "[1001]", viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var verifyRun = await verifyDb.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(verifyRun.IsPublished, Is.False,
            "curated register has no is_published wire field — server clears it to keep state consistent");
    }
}
