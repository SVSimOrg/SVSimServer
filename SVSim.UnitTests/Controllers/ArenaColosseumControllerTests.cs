using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Phase 1 lobby read coverage: /arena_colosseum/{top, get_fee_info, event_info}.
/// Defaults (no <c>ColosseumSeason</c> override) must render an empty "no event scheduled"
/// payload — flipping the season on is an admin operation.
/// </summary>
public class ArenaColosseumControllerTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    [Test]
    public async Task Top_unauthenticated_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();
        var resp = await client.PostAsync("/arena_colosseum/top", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Top_returns_no_period_when_no_season_active()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/arena_colosseum/top", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("\"is_colosseum_period\":false", body);
        // leader_skin_id must always be emitted (even when 0) per project_wire_null_policy.
        StringAssert.Contains("\"leader_skin_id\":0", body);
    }

    [Test]
    public async Task GetFeeInfo_returns_no_period_when_no_season_active()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/arena_colosseum/get_fee_info", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("\"is_colosseum_period\":false", body);
        // fee_list, is_unfinished_entry_exists, deck_format must be ABSENT when no event.
        StringAssert.DoesNotContain("\"fee_list\"", body);
        StringAssert.DoesNotContain("\"is_unfinished_entry_exists\"", body);
    }

    [Test]
    public async Task EventInfo_returns_empty_rounds_when_default_config()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/arena_colosseum/event_info", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();

        // The rounds object MUST be string-keyed "1"/"2"/"3" — locking the wire shape per
        // event_info.md. Custom STJ converter avoided; explicit [JsonPropertyName("1"|"2"|"3")]
        // produces the same on-the-wire bytes.
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.TryGetProperty("1", out var r1), Is.True, "round '1' must be present");
        Assert.That(root.TryGetProperty("2", out var r2), Is.True, "round '2' must be present");
        Assert.That(root.TryGetProperty("3", out var r3), Is.True, "round '3' must be present");

        // Default config → no schedule → is_now_round false on all three.
        Assert.That(r1.GetProperty("is_now_round").GetBoolean(), Is.False);
        Assert.That(r2.GetProperty("is_now_round").GetBoolean(), Is.False);
        Assert.That(r3.GetProperty("is_now_round").GetBoolean(), Is.False);

        Assert.That(r1.GetProperty("round_detail").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Top_round_trips_after_entry_seeded()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();

        // Seed an active run directly — Task 3's /entry endpoint will own creation, but
        // /top must reflect the row's identity when one exists.
        const long entryId = 12_345L;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
            {
                ViewerId = viewerId,
                EntryId = entryId,
                SeasonId = 1,
                RoundId = 1,
                DeckFormat = Format.Rotation,
                LeaderSkinId = 0,
                ConsumeItemType = 2,
                MaxBattleCountThisRound = 5,
                BreakthroughNumberThisRound = 4,
                RestEntryNum = 0,
                WinCount = 1,
                LossCount = 0,
                ResultListJson = "[1]",
            });
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/arena_colosseum/top", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("entry_info").GetProperty("id").GetInt64(), Is.EqualTo(entryId));
        Assert.That(root.GetProperty("now_round_id").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("max_battle_count").GetInt32(), Is.EqualTo(5));
        Assert.That(root.GetProperty("battle_results").GetProperty("win_count").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("battle_results").GetProperty("result_list").GetArrayLength(), Is.EqualTo(1));
    }
}
