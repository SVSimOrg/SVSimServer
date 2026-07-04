using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration;

/// <summary>
/// Phase 2 ship gate: a viewer walks the full bracket — entry → register_deck →
/// (battle_finish × N) → /finish (champion path), plus a separate retire-mid-round
/// variant. Exercises every Phase 2 endpoint plus Phase 1's /entry + /register_deck.
/// </summary>
public class ArenaColosseumEndToEndTests
{
    private static readonly object Envelope =
        new { viewer_id = "0", steam_id = 0, steam_session_ticket = "" };

    private static async Task ConfigureSeasonAsync(SVSimTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        // Season active, free entry allowed (so we don't have to seed currency).
        var seasonJson = JsonSerializer.Serialize(new
        {
            IsColosseumPeriod = true,
            SeasonId = 100,
            ColosseumName = "E2E Cup",
            DeckFormat = (int)Format.Rotation,
            CrystalCost = 0,
            RupyCost = 0,
            TicketCost = 0,
            IsAllowedFreeEntry = true,
            FinalRoundEliminateCount = 100,
        });
        await UpsertConfigAsync(db, "ColosseumSeason", seasonJson);

        // 3-round bracket. Round 1: BR=2 (clear with 2 wins). Round 2: BR=2. Round 3: BR=2.
        var roundsJson = JsonSerializer.Serialize(new
        {
            Rounds = new[]
            {
                BuildRound(1, breakthrough: 2, finishCount: 50),
                BuildRound(2, breakthrough: 2, finishCount: 100),
                BuildRound(3, breakthrough: 2, finishCount: 500),
            },
            ChampionRewards = new[]
            {
                new { Type = (int)UserGoodsType.Crystal, DetailId = 0L, Count = 9999, Name = "E2E Champion" },
            },
        });
        await UpsertConfigAsync(db, "ColosseumRounds", roundsJson);
    }

    private static object BuildRound(int roundId, int breakthrough, int finishCount) => new
    {
        RoundId = roundId,
        Groups = new[]
        {
            new
            {
                Group = $"R{roundId}",
                MaxBattleCount = 5,
                BreakthroughNumber = breakthrough,
                EntryNumber = 100,
            },
        },
        FinishRewards = new[]
        {
            new
            {
                Type = (int)UserGoodsType.Crystal,
                DetailId = 0L,
                Count = finishCount,
                Name = $"R{roundId} finish",
            },
        },
        RetireRewards = new object[]
        {
            new
            {
                Type = (int)UserGoodsType.Rupy,
                DetailId = 0L,
                Count = 10 * roundId,
                Name = $"R{roundId} retire",
            },
        },
    };

    private static async Task UpsertConfigAsync(SVSimDbContext db, string section, string json)
    {
        var existing = await db.GameConfigs.FirstOrDefaultAsync(s => s.SectionName == section);
        if (existing is null)
            db.GameConfigs.Add(new GameConfigSection { SectionName = section, ValueJson = json });
        else
            existing.ValueJson = json;
        await db.SaveChangesAsync();
    }

    private static HttpContent BattleFinishPayload(int battleResult, int isRetire) =>
        JsonContent.Create(new
        {
            battle_result = battleResult,
            is_retire = isRetire,
            class_id = 1,
            total_turn = 5,
            evolve_count = 1,
            enemy_evolve_count = 0,
            recovery_data = "",
            viewer_id = "0",
            steam_id = 0,
            steam_session_ticket = "",
        });

    private static async Task PromoteRunToRoundAsync(
        SVSimTestFactory factory, long viewerId, int newRoundId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstAsync(r => r.ViewerId == viewerId);
        run.RoundId = newRoundId;
        run.WinCount = 0;
        run.LossCount = 0;
        run.BattleCountThisRound = 0;
        run.ResultListJson = "[]";
        // Pull the per-round thresholds for the new round (mirrors what /entry copies on
        // initial create — Phase 2 doesn't have a real intra-bracket promotion endpoint yet).
        run.MaxBattleCountThisRound = 5;
        run.BreakthroughNumberThisRound = 2;
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Full_three_round_bracket_walk_ends_as_champion()
    {
        using var factory = new SVSimTestFactory();
        await ConfigureSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(vid, Format.Rotation, number: 1);
        using var client = factory.CreateAuthenticatedClient(vid);

        // /entry (free)
        var entryResp = await client.PostAsync("/arena_colosseum/entry",
            JsonContent.Create(new { consume_item_type = 5, now_round_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(entryResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "entry must succeed");

        // /register_deck
        var registerResp = await client.PostAsync("/arena_colosseum/register_deck",
            JsonContent.Create(new { deck_no_list = "[1]", is_published = true, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        Assert.That(registerResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "register_deck must succeed");

        // Round 1: 2 wins → clears breakthrough.
        for (int i = 0; i < 2; i++)
        {
            var r = await client.PostAsync("/colosseum_battle/finish", BattleFinishPayload(battleResult: 1, isRetire: 0));
            Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"R1 battle {i} must succeed");
        }
        await PromoteRunToRoundAsync(factory, vid, newRoundId: 2);

        // Round 2: 2 wins.
        for (int i = 0; i < 2; i++)
        {
            var r = await client.PostAsync("/colosseum_battle/finish", BattleFinishPayload(battleResult: 1, isRetire: 0));
            Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"R2 battle {i} must succeed");
        }
        await PromoteRunToRoundAsync(factory, vid, newRoundId: 3);

        // Round 3: 2 wins → champion path on /finish.
        for (int i = 0; i < 2; i++)
        {
            var r = await client.PostAsync("/colosseum_battle/finish", BattleFinishPayload(battleResult: 1, isRetire: 0));
            Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"R3 battle {i} must succeed");
        }

        // /arena_colosseum/finish (champion path)
        var finishResp = await client.PostAsync("/arena_colosseum/finish", JsonContent.Create(Envelope));
        Assert.That(finishResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await finishResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("colosseum_status").GetProperty("is_champion").GetBoolean(), Is.True);
        // Final R3 finish reward + champion bundle = 2 entries.
        Assert.That(root.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(2));

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var run = await db.ViewerArenaColosseumRuns.FirstOrDefaultAsync(r => r.ViewerId == vid);
        Assert.That(run, Is.Null, "champion run must be deleted on /finish");
    }

    [Test]
    public async Task Retire_during_round_2_emits_round_capped_consolation()
    {
        using var factory = new SVSimTestFactory();
        await ConfigureSeasonAsync(factory);
        var vid = await factory.SeedViewerAsync();
        await factory.SeedDeckAsync(vid, Format.Rotation, number: 1);
        using var client = factory.CreateAuthenticatedClient(vid);

        // entry + register_deck + clear R1 + promote
        await client.PostAsync("/arena_colosseum/entry",
            JsonContent.Create(new { consume_item_type = 5, now_round_id = 1, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        await client.PostAsync("/arena_colosseum/register_deck",
            JsonContent.Create(new { deck_no_list = "[1]", is_published = false, viewer_id = "0", steam_id = 0, steam_session_ticket = "" }));
        for (int i = 0; i < 2; i++)
            await client.PostAsync("/colosseum_battle/finish", BattleFinishPayload(battleResult: 1, isRetire: 0));
        await PromoteRunToRoundAsync(factory, vid, newRoundId: 2);

        // One mid-round battle, then retire.
        await client.PostAsync("/colosseum_battle/finish", BattleFinishPayload(battleResult: 2, isRetire: 0));

        var retireResp = await client.PostAsync("/arena_colosseum/retire", JsonContent.Create(Envelope));
        Assert.That(retireResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await retireResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var rewards = doc.RootElement.GetProperty("rewards");
        Assert.That(rewards.GetArrayLength(), Is.EqualTo(1));
        Assert.That(rewards[0].GetProperty("name").GetString(), Is.EqualTo("R2 retire"),
            "retire payload must reflect the round the viewer was IN, not their starting round");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await db.ViewerArenaColosseumRuns.AnyAsync(r => r.ViewerId == vid), Is.False);
    }
}
