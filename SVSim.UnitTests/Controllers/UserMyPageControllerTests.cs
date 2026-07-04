using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class UserMyPageControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<Viewer> LoadViewerWithRotation(SVSimTestFactory factory, long viewerId)
    {
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        return await ctx.Viewers
            .Include(v => v.MyPageBgRotation)
            .AsNoTracking()
            .FirstAsync(v => v.Id == viewerId);
    }

    [Test]
    public async Task Update_persists_select_type_and_single_bg()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""
            {"select_type":1,"mypage_id":"1213410310","mypage_id_list":[]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        var viewer = await LoadViewerWithRotation(factory, viewerId);
        Assert.That(viewer.MyPageBgSelectType, Is.EqualTo(1));
        Assert.That(viewer.MyPageBgId, Is.EqualTo(1213410310));
        Assert.That(viewer.MyPageBgRotation, Is.Empty);
    }

    [Test]
    public async Task Update_persists_rotation_pool_in_slot_order()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""
            {"select_type":2,"mypage_id":"0","mypage_id_list":["1211410310","1212410310","1213410310"]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var viewer = await LoadViewerWithRotation(factory, viewerId);
        var pool = viewer.MyPageBgRotation.OrderBy(r => r.Slot).Select(r => r.BgId).ToList();
        Assert.That(pool, Is.EqualTo(new[] { 1211410310, 1212410310, 1213410310 }));
    }

    [Test]
    public async Task Update_overwrites_previous_rotation_atomically()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        // Seed a 5-entry rotation directly.
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await ctx.Viewers.Include(v => v.MyPageBgRotation).FirstAsync(v => v.Id == viewerId);
            for (int slot = 0; slot < 5; slot++)
            {
                viewer.MyPageBgRotation.Add(new MyPageBgRotationEntry { Slot = slot, BgId = 9000 + slot });
            }
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var body = JsonBody("""
            {"select_type":2,"mypage_id":"0","mypage_id_list":["1001","1002","1003"]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var viewer2 = await LoadViewerWithRotation(factory, viewerId);
        var pool = viewer2.MyPageBgRotation.OrderBy(r => r.Slot).Select(r => r.BgId).ToList();
        Assert.That(pool, Is.EqualTo(new[] { 1001, 1002, 1003 }),
            "old slots 3-4 should have been deleted, not orphaned");
    }

    [Test]
    public async Task Update_with_empty_mypage_id_falls_back_to_zero()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""
            {"select_type":0,"mypage_id":"","mypage_id_list":[]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var viewer = await LoadViewerWithRotation(factory, viewerId);
        Assert.That(viewer.MyPageBgSelectType, Is.EqualTo(0));
        Assert.That(viewer.MyPageBgId, Is.EqualTo(0));
    }

    [Test]
    public async Task Update_with_unparseable_mypage_id_falls_back_to_zero()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""
            {"select_type":0,"mypage_id":"garbage","mypage_id_list":[]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var viewer = await LoadViewerWithRotation(factory, viewerId);
        Assert.That(viewer.MyPageBgId, Is.EqualTo(0));
    }

    [Test]
    public async Task Update_returns_envelope_with_empty_data_payload()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""
            {"select_type":0,"mypage_id":"0","mypage_id_list":[]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        // Test path bypasses the translation middleware (gated on UnityPlayer UA), so the
        // controller's literal return value is what comes back. An empty class serializes to "{}".
        Assert.That(raw, Is.EqualTo("{}"));
    }

    [Test]
    public async Task Update_with_unparseable_mypage_id_list_entries_falls_back_to_zero()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""
            {"select_type":2,"mypage_id":"0","mypage_id_list":["1001","garbage","","1002"]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var viewer = await LoadViewerWithRotation(factory, viewerId);
        var pool = viewer.MyPageBgRotation.OrderBy(r => r.Slot).Select(r => r.BgId).ToList();
        Assert.That(pool, Is.EqualTo(new[] { 1001, 0, 0, 1002 }),
            "garbage and empty entries are stored as 0; valid entries unaffected");
    }

    [Test]
    public async Task MyPageIndex_for_fresh_viewer_returns_zero_defaults()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var body = JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var response = await client.PostAsync("/mypage/index", body);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var setting = doc.RootElement.GetProperty("user_mypage_info").GetProperty("user_mypage_setting");
        Assert.That(setting.GetProperty("mypage_id").GetString(), Is.EqualTo("0"));
        Assert.That(setting.GetProperty("select_type").GetString(), Is.EqualTo("0"));
        Assert.That(setting.GetProperty("mypage_id_list").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Update_then_MyPageIndex_round_trips_full_state()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var updateBody = JsonBody("""
            {"select_type":1,"mypage_id":"1213410310","mypage_id_list":["1211410310","1212410310","1213410310","1214410310","1215410310","1216410310","1217410310","1218410310"]}
            """);
        var updateResp = await client.PostAsync("/user_mypage/update", updateBody);
        Assert.That(updateResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var indexBody = JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""");
        var indexResp = await client.PostAsync("/mypage/index", indexBody);
        var raw = await indexResp.Content.ReadAsStringAsync();
        Assert.That(indexResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var setting = doc.RootElement.GetProperty("user_mypage_info").GetProperty("user_mypage_setting");
        Assert.That(setting.GetProperty("mypage_id").GetString(), Is.EqualTo("1213410310"));
        Assert.That(setting.GetProperty("select_type").GetString(), Is.EqualTo("1"));
        var list = setting.GetProperty("mypage_id_list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(8));
        Assert.That(list[0].GetString(), Is.EqualTo("1211410310"));
        Assert.That(list[7].GetString(), Is.EqualTo("1218410310"));
    }

    [Test]
    public async Task Update_without_auth_returns_401()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateClient();  // NO X-Test-Viewer-Id header

        var body = JsonBody("""
            {"select_type":0,"mypage_id":"0","mypage_id_list":[]}
            """);
        var response = await client.PostAsync("/user_mypage/update", body);
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }
}
