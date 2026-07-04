using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class LeaderSkinControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    /// <summary>Adds a class-4 leader skin (id 104, "Forte") to the catalog and to the viewer's owned list.</summary>
    private static async Task SeedOwnedClass4Skin(SVSimTestFactory f, long viewerId, int skinId = 104)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var skin = await db.LeaderSkins.FindAsync(skinId);
        if (skin is null)
        {
            skin = new LeaderSkinEntry { Id = skinId, Name = "Forte", ClassId = 4 };
            db.LeaderSkins.Add(skin);
            await db.SaveChangesAsync();
        }
        var viewer = await db.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
        if (viewer.LeaderSkins.All(s => s.Id != skinId)) viewer.LeaderSkins.Add(skin);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Set_updates_viewer_class_leader_skin()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedOwnedClass4Skin(factory, viewerId, skinId: 104);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","class_id":4,"leader_skin_id":104,"is_random_leader_skin":false,"leader_skin_id_list":[]}""";
        var response = await client.PostAsync("/leader_skin/set", JsonBody(json));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(v => v.Classes).ThenInclude(c => c.LeaderSkin)
            .Include(v => v.Classes).ThenInclude(c => c.Class)
            .FirstAsync(v => v.Id == viewerId);
        var class4 = viewer.Classes.Single(c => c.Class.Id == 4);
        Assert.That(class4.LeaderSkin.Id, Is.EqualTo(104));
    }

    [Test]
    public async Task Set_is_reflected_in_subsequent_deck_info_response()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedOwnedClass4Skin(factory, viewerId, skinId: 104);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Switch class 4 leader to skin 104
        await client.PostAsync("/leader_skin/set",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","class_id":4,"leader_skin_id":104,"is_random_leader_skin":false,"leader_skin_id_list":[]}"""));

        // /deck/info should now report class 4 with leader_skin_id=104
        var resp = await client.PostAsync("/deck/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","deck_format":0}"""));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        using var doc = JsonDocument.Parse(body);
        var settings = doc.RootElement.GetProperty("user_leader_skin_setting_list");
        Assert.That(settings.TryGetProperty("4", out var class4Setting), Is.True, "class 4 entry must be present");
        Assert.That(class4Setting.GetProperty("leader_skin_id").GetInt32(), Is.EqualTo(104));
    }

    [Test]
    public async Task Set_rejects_skin_viewer_doesnt_own()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Skin 104 (Forte) is in the seeded leaderskins.csv catalog but a fresh viewer only owns
        // the 8 class default skins — confirm 104 isn't in viewer.LeaderSkins, then call /set.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var skin = await db.LeaderSkins.FindAsync(104);
            Assert.That(skin, Is.Not.Null, "leaderskins.csv fixture should include skin 104");
            var viewer = await db.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
            Assert.That(viewer.LeaderSkins.Any(s => s.Id == 104), Is.False, "fresh viewer must not own skin 104");
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","class_id":4,"leader_skin_id":104,"is_random_leader_skin":false,"leader_skin_id_list":[]}""";
        var resp = await client.PostAsync("/leader_skin/set", JsonBody(json));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await resp.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Set_rejects_skin_for_wrong_class()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // Skin 104 is class 4 — try to assign it to class 6
        await SeedOwnedClass4Skin(factory, viewerId, skinId: 104);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","class_id":6,"leader_skin_id":104,"is_random_leader_skin":false,"leader_skin_id_list":[]}""";
        var resp = await client.PostAsync("/leader_skin/set", JsonBody(json));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Set_returns_501_for_random_leader_skin_mode()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","class_id":4,"leader_skin_id":0,"is_random_leader_skin":true,"leader_skin_id_list":[4,104]}""";
        var resp = await client.PostAsync("/leader_skin/set", JsonBody(json));
        Assert.That((int)resp.StatusCode, Is.EqualTo(501));
    }

    [Test]
    public async Task Set_freeplay_allows_equipping_unowned_skin()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        await factory.EnableFreeplayAsync();

        int classId, skinId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var skin = await db.LeaderSkins.FirstAsync(s => s.ClassId != null);
            skinId = skin.Id; classId = skin.ClassId!.Value;
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var json = $$"""{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","class_id":{{classId}},"leader_skin_id":{{skinId}},"is_random_leader_skin":false,"leader_skin_id_list":[]}""";
        var resp = await client.PostAsync("/leader_skin/set", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.That(resp.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());
    }
}
