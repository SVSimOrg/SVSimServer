using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class PuzzleControllerTests
{
    private const string BaseRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    [Test]
    public async Task Info_returns_25_groups_with_puzzles()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/basic_puzzle/info",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        // Controllers return the inner data payload; the wrapping {data_headers, data} envelope
        // is added by ShadowverseTranslationMiddleware which the test factory bypasses, so the
        // root element here IS the array (see PracticeControllerTests for the same pattern).
        var data = doc.RootElement;
        Assert.That(data.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(data.GetArrayLength(), Is.EqualTo(25));

        var g301 = data.EnumerateArray().Single(g => g.GetProperty("puzzle_master_id").GetString() == "301");
        Assert.That(g301.GetProperty("is_all_cleared").GetBoolean(), Is.False);
        Assert.That(g301.GetProperty("puzzle_data").GetArrayLength(), Is.EqualTo(3));
        // String-on-wire assertion: puzzle_master_id ships as a JSON string, not number.
        Assert.That(g301.GetProperty("puzzle_master_id").ValueKind, Is.EqualTo(JsonValueKind.String));
    }

    [Test]
    public async Task Info_reflects_per_viewer_clears()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        // Resolve the repo from a scope, not factory.Services directly (scoped service constraint).
        using (var scope = factory.Services.CreateScope())
        {
            var clearRepo = scope.ServiceProvider.GetRequiredService<IPuzzleClearRepository>();
            await clearRepo.UpsertClearAsync(viewerId, 37, 0);
        }
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = await (await client.PostAsync("/basic_puzzle/info",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json")))
            .Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var g301 = doc.RootElement.EnumerateArray()
            .Single(g => g.GetProperty("puzzle_master_id").GetString() == "301");
        var p37 = g301.GetProperty("puzzle_data").EnumerateArray()
            .Single(p => p.GetProperty("puzzle_id").GetString() == "37");
        Assert.That(p37.GetProperty("is_cleared").GetBoolean(), Is.True);

        Assert.That(g301.GetProperty("is_mission_target").GetBoolean(), Is.True,
            "Round 1 mission still incomplete (1/3) so group 301 is still a mission target");
    }

    [Test]
    public async Task OpenPuzzleDialog_returns_one_group()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","puzzle_master_id":301}""";
        var body = await (await client.PostAsync("/basic_puzzle/open_puzzle_dialog",
            new StringContent(req, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("puzzle_quest").GetArrayLength(), Is.EqualTo(3));
        Assert.That(root.GetProperty("puzzle_quest_chara_id").GetString(), Is.EqualTo("3704"));
        Assert.That(root.GetProperty("is_display_badge").GetBoolean(), Is.False);
        Assert.That(root.GetProperty("is_display_puzzle_new").GetBoolean(), Is.False);
    }

    [Test]
    public async Task OpenPuzzleDialog_unknown_group_returns_empty_payload()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","puzzle_master_id":99999}""";
        var resp = await client.PostAsync("/basic_puzzle/open_puzzle_dialog",
            new StringContent(req, Encoding.UTF8, "application/json"));

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        Assert.That(root.GetProperty("puzzle_quest").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Start_returns_empty_array()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","puzzle_id":1}""";
        var body = await (await client.PostAsync("/basic_puzzle/start",
            new StringContent(req, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Mission_returns_19_entries_ordered_and_progress_tracked()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        // Clear 2 of 3 puzzles in group 301 (the Round-1 mission target).
        using (var scope = factory.Services.CreateScope())
        {
            var clearRepo = scope.ServiceProvider.GetRequiredService<IPuzzleClearRepository>();
            await clearRepo.UpsertClearAsync(viewerId, 37, 0);
            await clearRepo.UpsertClearAsync(viewerId, 38, 0);
        }
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = await (await client.PostAsync("/basic_puzzle/mission",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement;
        Assert.That(data.GetArrayLength(), Is.EqualTo(19));

        var round1 = data.EnumerateArray()
            .Single(m => m.GetProperty("mission_name").GetString() == "Clear all Round 1 puzzles");
        Assert.That(round1.GetProperty("total_count").GetString(), Is.EqualTo("2"));
        Assert.That(round1.GetProperty("require_number").GetString(), Is.EqualTo("3"));
        Assert.That(round1.GetProperty("is_achieved").GetBoolean(), Is.False);

        var special = data.EnumerateArray()
            .Single(m => m.GetProperty("mission_name").GetString() == "Clear all Special Round puzzles");
        Assert.That(special.GetProperty("total_count").GetString(), Is.EqualTo("0"),
            "Special-Round missions always surface as 0 in Phase 1");
    }

    [Test]
    public async Task Finish_loss_is_stateless_and_returns_loss_shape()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","puzzle_id":37,"retry_count":0,"is_win":false}""";
        var body = await (await client.PostAsync("/basic_puzzle/finish",
            new StringContent(req, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement;

        // Loss-specific: win_count is the NUMBER 0, not the string "1".
        Assert.That(data.GetProperty("win_count").ValueKind, Is.EqualTo(JsonValueKind.Number));
        Assert.That(data.GetProperty("win_count").GetInt32(), Is.EqualTo(0));
        Assert.That(data.GetProperty("achieved_info").GetProperty("mission_start_data").GetArrayLength(), Is.EqualTo(0));
        Assert.That(data.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(0));

        // No DB writes.
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx.ViewerPuzzleClears.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task Finish_win_persists_clear_and_returns_win_shape()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","puzzle_id":37,"retry_count":0,"is_win":true}""";
        var body = await (await client.PostAsync("/basic_puzzle/finish",
            new StringContent(req, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement;

        // Win-specific: win_count is the STRING "1".
        Assert.That(data.GetProperty("win_count").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(data.GetProperty("win_count").GetString(), Is.EqualTo("1"));

        // 1/3 in group 301 → no mission completion yet.
        Assert.That(data.GetProperty("achieved_info").GetProperty("achieved_mission_list").GetArrayLength(), Is.EqualTo(0));
        Assert.That(data.GetProperty("reward_list").GetArrayLength(), Is.EqualTo(0));

        // mission_start_data still contains the un-achieved Round-1 mission.
        var starts = data.GetProperty("achieved_info").GetProperty("mission_start_data");
        Assert.That(starts.EnumerateArray().Any(e => e.GetProperty("mission_name").GetString() == "Clear all Round 1 puzzles"), Is.True);

        // Clear was persisted.
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx.ViewerPuzzleClears.AnyAsync(c => c.ViewerId == viewerId && c.PuzzleId == 37), Is.True);
    }

    [Test]
    public async Task Finish_completes_mission_grants_reward_and_toggles_mission_target()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();

        // The Round-1 mission rewards LeaderSkin 3704. SeedGlobalsAsync's leaderskins.csv may
        // already include this id; insert defensively (skip if exists) so the test is
        // independent of seed data shape.
        using (var setup = factory.Services.CreateScope())
        {
            var ctx = setup.ServiceProvider.GetRequiredService<SVSimDbContext>();
            if (!await ctx.LeaderSkins.AnyAsync(s => s.Id == 3704))
            {
                ctx.LeaderSkins.Add(new LeaderSkinEntry { Id = 3704, Name = "Round1Reward" });
                await ctx.SaveChangesAsync();
            }

            var clearRepo = setup.ServiceProvider.GetRequiredService<IPuzzleClearRepository>();
            await clearRepo.UpsertClearAsync(viewerId, 37, 0);
            await clearRepo.UpsertClearAsync(viewerId, 38, 0);
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var req = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","puzzle_id":39,"retry_count":0,"is_win":true}""";
        var body = await (await client.PostAsync("/basic_puzzle/finish",
            new StringContent(req, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement;
        var ai = data.GetProperty("achieved_info");

        // Achievement banner emitted.
        Assert.That(ai.GetProperty("achieved_mission_list").GetArrayLength(), Is.EqualTo(1));
        Assert.That(ai.GetProperty("achieved_mission_list")[0].GetProperty("achieved_message").GetString(),
            Is.EqualTo("Cleared all Round 1 puzzles"));

        // mission_reward_* prefixed shape (NOT reward_detail_id/number).
        var mrl = ai.GetProperty("achieved_mission_reward_list");
        Assert.That(mrl.GetArrayLength(), Is.EqualTo(1));
        Assert.That(mrl[0].GetProperty("mission_reward_type").GetString(), Is.EqualTo("10"));
        Assert.That(mrl[0].GetProperty("mission_reward_detail_id").GetString(), Is.EqualTo("3704"));

        // Top-level reward_list mirrors as TreasureReward shape (reward_id / reward_num).
        var rl = data.GetProperty("reward_list");
        Assert.That(rl.GetArrayLength(), Is.EqualTo(1));
        Assert.That(rl[0].GetProperty("reward_id").GetString(), Is.EqualTo("3704"));
        Assert.That(rl[0].GetProperty("reward_num").GetString(), Is.EqualTo("1"));

        // Viewer collection updated — owns the leader skin now.
        using var verify = factory.Services.CreateScope();
        var verifyCtx = verify.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await verifyCtx.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.LeaderSkins.Any(s => s.Id == 3704), Is.True);

        // mission_start_data no longer contains the achieved Round-1 mission.
        var starts = ai.GetProperty("mission_start_data");
        Assert.That(starts.EnumerateArray().Any(e => e.GetProperty("mission_name").GetString() == "Clear all Round 1 puzzles"),
            Is.False);

        // puzzle_list entry for group 301 has is_mission_target=false now.
        var g301 = data.GetProperty("puzzle_list").EnumerateArray()
            .Single(g => g.GetProperty("puzzle_master_id").GetString() == "301");
        Assert.That(g301.GetProperty("is_all_cleared").GetBoolean(), Is.True);
        Assert.That(g301.GetProperty("is_mission_target").GetBoolean(), Is.False);
    }
}
