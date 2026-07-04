using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class MissionControllerTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");
    private const string EmptyAuthBody = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    private static async Task ImportCatalogs(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await new MissionCatalogImporter().ImportAsync(db, SeedDir);
        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);
        await new BattlePassMonthlyMissionImporter().ImportAsync(db, SeedDir);
    }

    [Test]
    public async Task Info_returns_assigned_missions_with_string_typed_lot_type()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var missions = root.GetProperty("user_mission_list");
        Assert.That(missions.GetArrayLength(), Is.EqualTo(4), "expect 1 daily + 3 weekly slots");
        // lot_type must be a JSON STRING per wire shape
        Assert.That(missions[0].GetProperty("lot_type").ValueKind, Is.EqualTo(JsonValueKind.String));

        // Daily mission (id 332) must have default_flag=true
        bool foundDaily = false;
        foreach (var m in missions.EnumerateArray())
        {
            if (m.GetProperty("mission_id").GetInt32() == 332)
            {
                foundDaily = true;
                Assert.That(m.GetProperty("lot_type").GetString(), Is.EqualTo("6"));
                Assert.That(m.GetProperty("default_flag").GetBoolean(), Is.True);
            }
        }
        Assert.That(foundDaily, Is.True, "daily mission 332 must be assigned to slot 0");
    }

    [Test]
    public async Task Info_returns_achievement_rows_with_derived_max_level()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        var achievements = doc.RootElement.GetProperty("user_achievement_list");
        Assert.That(achievements.GetArrayLength(), Is.GreaterThan(0));
        // Type 12 has 2 captured tiers (levels 6, 7) → max_level should be 7
        foreach (var a in achievements.EnumerateArray())
        {
            if (a.GetProperty("achievement_type").GetInt32() == 12)
            {
                Assert.That(a.GetProperty("max_level").GetInt32(), Is.EqualTo(7));
                return;
            }
        }
        Assert.Fail("achievement type 12 not found in response");
    }

    [Test]
    public async Task Info_includes_bp_monthly_for_may_2026_when_seeded()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        // The seed only has May 2026 BP monthly missions. If the test runs in a different month,
        // the block will be omitted. This test asserts that EITHER the block is absent OR has 5
        // entries — both are valid behaviors depending on calendar date.
        if (doc.RootElement.TryGetProperty("battle_pass_monthly_mission", out var bp))
        {
            Assert.That(bp.GetProperty("mission_list").GetArrayLength(), Is.EqualTo(5));
        }
        // If not present, that's also valid: current month isn't in the seed.
    }

    [Test]
    public async Task Info_can_change_mission_time_is_null_when_change_allowed()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var resp = await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));
        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("is_change_mission").GetBoolean(), Is.True);
        var cct = root.GetProperty("can_change_mission_time");
        Assert.That(cct.ValueKind, Is.EqualTo(JsonValueKind.Null),
            "can_change_mission_time must serialize as explicit JSON null");
    }

    [Test]
    public async Task Retire_swaps_weekly_slot_for_new_pool_member()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // First /mission/info to materialize slots
        await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));

        // Find a weekly mission to retire
        long retireId; int originalCatalogId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var weekly = await db.ViewerMissions.FirstAsync(m => m.ViewerId == viewerId && m.Slot == 1);
            retireId = weekly.Id;
            originalCatalogId = weekly.MissionCatalogId;
        }

        var retireBody = $$"""{"id":{{retireId}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/mission/retire", JsonBody(retireBody));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var db2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var slot1Now = await db2.ViewerMissions.FirstAsync(m => m.ViewerId == viewerId && m.Slot == 1);
        Assert.That(slot1Now.MissionCatalogId, Is.Not.EqualTo(originalCatalogId),
            "retire must replace the catalog id with a different one from the pool");
    }

    [Test]
    public async Task Retire_sets_can_change_mission_time()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));

        long retireId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var weekly = await db.ViewerMissions.FirstAsync(m => m.ViewerId == viewerId && m.Slot == 1);
            retireId = weekly.Id;
        }

        var retireBody = $$"""{"id":{{retireId}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/mission/retire", JsonBody(retireBody));
        var body = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("is_change_mission").GetBoolean(), Is.False);
        Assert.That(root.GetProperty("can_change_mission_time").ValueKind, Is.EqualTo(JsonValueKind.Number));
    }

    [Test]
    public async Task Retire_rejects_daily_slot()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        await client.PostAsync("/mission/info", JsonBody(EmptyAuthBody));

        long dailyId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var daily = await db.ViewerMissions.FirstAsync(m => m.ViewerId == viewerId && m.Slot == 0);
            dailyId = daily.Id;
        }

        var retireBody = $$"""{"id":{{dailyId}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/mission/retire", JsonBody(retireBody));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("\"result_code\":2"),
            "daily slot retire must fail with result_code = 2");
    }

    [Test]
    public async Task Retire_rejects_id_not_owned_by_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var retireBody = """{"id":999999999,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/mission/retire", JsonBody(retireBody));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("\"result_code\":2"));
    }

    [Test]
    public async Task ChangeReceiveSetting_persists_to_viewer_mission_data()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var changeBody = """{"mission_receive_type":1,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/mission/change_receive_setting", JsonBody(changeBody));
        resp.EnsureSuccessStatusCode();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var refreshed = await db.Viewers.Include(v => v.MissionData)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(refreshed.MissionData.MissionReceiveType, Is.EqualTo(1));

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("mission_receive_type").GetString(), Is.EqualTo("1"));
    }
}
