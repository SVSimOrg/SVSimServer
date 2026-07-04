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
/// Bracket-end /finish + /retire coverage. Locks the wire shape (rewards + reward_list +
/// colosseum_status) and verifies side-effects: rewards granted through the inventory
/// service, run row deleted, champion flag flipped on final-round clear.
/// </summary>
public class ArenaColosseumControllerBracketTerminateTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    /// <summary>Three-round config with reward bundles on every round + a champion bundle.</summary>
    private static async Task ActivateSeasonWithRewardsAsync(SVSimTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var seasonJson = JsonSerializer.Serialize(new
        {
            IsColosseumPeriod = true,
            SeasonId = 42,
            ColosseumName = "Test Cup",
            DeckFormat = (int)Format.Rotation,
            FinalRoundEliminateCount = 1000,
        });
        await UpsertConfigAsync(db, "ColosseumSeason", seasonJson);

        var roundsJson = JsonSerializer.Serialize(new
        {
            Rounds = new[]
            {
                new
                {
                    RoundId = 1,
                    Groups = new[] { new { Group = "", MaxBattleCount = 5, BreakthroughNumber = 3, EntryNumber = 100_000 } },
                    FinishRewards = new[]
                    {
                        new { Type = (int)UserGoodsType.Crystal, DetailId = 0L, Count = 100, Name = "R1 finish" },
                    },
                    RetireRewards = new object[]
                    {
                        new { Type = (int)UserGoodsType.Rupy, DetailId = 0L, Count = 50, Name = "R1 retire" },
                    },
                },
                new
                {
                    RoundId = 2,
                    Groups = new[] { new { Group = "Group A", MaxBattleCount = 5, BreakthroughNumber = 4, EntryNumber = 10_000 } },
                    FinishRewards = new[]
                    {
                        new { Type = (int)UserGoodsType.Crystal, DetailId = 0L, Count = 250, Name = "R2 finish" },
                    },
                    RetireRewards = Array.Empty<object>(),
                },
                new
                {
                    RoundId = 3,
                    Groups = new[] { new { Group = "Final", MaxBattleCount = 5, BreakthroughNumber = 4, EntryNumber = 1_000 } },
                    FinishRewards = new[]
                    {
                        new { Type = (int)UserGoodsType.Crystal, DetailId = 0L, Count = 1000, Name = "Final clear" },
                    },
                    RetireRewards = Array.Empty<object>(),
                },
            },
            ChampionRewards = new[]
            {
                new { Type = (int)UserGoodsType.Crystal, DetailId = 0L, Count = 5000, Name = "Champion Pack" },
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

    private static async Task SeedRunAsync(
        SVSimTestFactory factory, long viewerId, int roundId, int winCount, int battleCount)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.ViewerArenaColosseumRuns.Add(new ViewerArenaColosseumRun
        {
            ViewerId = viewerId,
            EntryId = 1001,
            SeasonId = 42,
            RoundId = roundId,
            DeckFormat = Format.Rotation,
            WinCount = winCount,
            BattleCountThisRound = battleCount,
            MaxBattleCountThisRound = 5,
            BreakthroughNumberThisRound = roundId == 1 ? 3 : 4,
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Finish_emits_champion_flag_when_round_3_cleared()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonWithRewardsAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid, roundId: 3, winCount: 4, battleCount: 4);

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/finish", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("colosseum_status").GetProperty("is_champion").GetBoolean(), Is.True);
        Assert.That(root.GetProperty("colosseum_status").GetProperty("colosseum_name").GetString(),
            Is.EqualTo("Test Cup"));

        // Final clear + champion bundle = 2 reward entries.
        Assert.That(root.GetProperty("rewards").GetArrayLength(), Is.EqualTo(2));
        Assert.That(root.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(2));
    }

    [Test]
    public async Task Finish_grants_rewards_and_deletes_run()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonWithRewardsAsync(factory);
        var vid = await factory.SeedViewerAsync();
        // Round 1 breakthrough advances to round 2 — bracket isn't finished. Use round 3 +
        // exhausted battle cap WITHOUT breakthrough to terminate cleanly with round-end rewards.
        await SeedRunAsync(factory, vid, roundId: 3, winCount: 2, battleCount: 5);

        ulong crystalsBefore;
        using (var beforeScope = factory.Services.CreateScope())
        {
            var beforeDb = beforeScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            crystalsBefore = (await beforeDb.Viewers.FirstAsync(v => v.Id == vid)).Currency.Crystals;
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/finish", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var run = await db.ViewerArenaColosseumRuns.FirstOrDefaultAsync(r => r.ViewerId == vid);
        Assert.That(run, Is.Null, "run row must be deleted on /finish");

        var viewer = await db.Viewers.FirstAsync(v => v.Id == vid);
        Assert.That(viewer.Currency.Crystals - crystalsBefore, Is.EqualTo(1000UL),
            "Round 3 finish bundle is 1000 Crystal (not champion — battle cap hit without breakthrough)");
    }

    [Test]
    public async Task Finish_rejects_when_bracket_still_in_progress()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonWithRewardsAsync(factory);
        var vid = await factory.SeedViewerAsync();
        // Mid-round: 1 win, 1 battle, breakthrough is 3 — not yet eligible.
        await SeedRunAsync(factory, vid, roundId: 1, winCount: 1, battleCount: 1);

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/finish", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await resp.Content.ReadAsStringAsync();
        StringAssert.Contains("bracket_not_finished", body);
    }

    [Test]
    public async Task Retire_grants_round_capped_rewards_and_deletes_run()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonWithRewardsAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await SeedRunAsync(factory, vid, roundId: 1, winCount: 1, battleCount: 2);

        ulong rupeesBefore;
        using (var beforeScope = factory.Services.CreateScope())
        {
            var beforeDb = beforeScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            rupeesBefore = (await beforeDb.Viewers.FirstAsync(v => v.Id == vid)).Currency.Rupees;
        }

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/retire", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("rewards").GetArrayLength(), Is.EqualTo(1));
        Assert.That(root.GetProperty("rewards")[0].GetProperty("name").GetString(), Is.EqualTo("R1 retire"));

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstOrDefaultAsync(r => r.ViewerId == vid);
        Assert.That(run, Is.Null, "run row must be deleted on /retire");

        var viewer = await db.Viewers.FirstAsync(v => v.Id == vid);
        Assert.That(viewer.Currency.Rupees - rupeesBefore, Is.EqualTo(50UL),
            "Round 1 retire bundle adds 50 Rupy on top of starting balance");
    }

    [Test]
    public async Task Retire_during_final_round_still_works_and_emits_status()
    {
        using var factory = new SVSimTestFactory();
        await ActivateSeasonWithRewardsAsync(factory);
        var vid = await factory.SeedViewerAsync();
        // Round 3 has empty RetireRewards per config — client ignores rewards at FinalB anyway.
        await SeedRunAsync(factory, vid, roundId: 3, winCount: 2, battleCount: 3);

        using var client = factory.CreateAuthenticatedClient(vid);
        var resp = await client.PostAsync("/arena_colosseum/retire", JsonContent.Create(Envelope));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("rewards").GetArrayLength(), Is.EqualTo(0),
            "FinalB retire emits empty rewards per retire.md");
        Assert.That(doc.RootElement.GetProperty("colosseum_status").GetProperty("now_round_id").GetInt32(),
            Is.EqualTo(3));
    }
}
