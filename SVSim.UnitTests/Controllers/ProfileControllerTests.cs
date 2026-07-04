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

public class ProfileControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    [Test]
    public async Task Index_returns_zero_total_wins_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/profile/index", JsonBody("{}"));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("user_rank_match_total_win").GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public async Task Index_returns_all_viewer_classes()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/profile/index", JsonBody("{}"));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var classes = doc.RootElement.GetProperty("user_class_list");
        Assert.That(classes.GetArrayLength(), Is.GreaterThanOrEqualTo(8),
            "fresh viewer should have at least 8 main-class entries");
        var classIds = Enumerable.Range(0, classes.GetArrayLength())
            .Select(i => classes[i].GetProperty("class_id").GetInt32())
            .ToList();
        Assert.That(classIds, Does.Contain(1));
        Assert.That(classIds, Does.Contain(8));
    }

    [Test]
    public async Task Index_class_entry_carries_level_exp_default_skin()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        // Seed level + exp on class 1.
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await ctx.Viewers.Include(v => v.Classes).ThenInclude(c => c.Class).FirstAsync(v => v.Id == viewerId);
            var cls1 = viewer.Classes.First(c => c.Class.Id == 1);
            cls1.Level = 5;
            cls1.Exp = 600;
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/profile/index", JsonBody("{}"));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var entry = doc.RootElement.GetProperty("user_class_list")
            .EnumerateArray()
            .First(e => e.GetProperty("class_id").GetInt32() == 1);
        Assert.That(entry.GetProperty("level").GetInt32(), Is.EqualTo(5));
        Assert.That(entry.GetProperty("exp").GetInt32(), Is.EqualTo(600));
        Assert.That(entry.GetProperty("default_leader_skin_id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Index_leader_skin_id_list_populated_from_owned_skins()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        // Grant the viewer two additional leader skins for class 1.
        const int extraSkinA = 10001;
        const int extraSkinB = 10002;
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.LeaderSkins.Add(new LeaderSkinEntry { Id = extraSkinA, Name = "extraA", ClassId = 1 });
            ctx.LeaderSkins.Add(new LeaderSkinEntry { Id = extraSkinB, Name = "extraB", ClassId = 1 });
            await ctx.SaveChangesAsync();

            var viewer = await ctx.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
            viewer.LeaderSkins.Add(await ctx.LeaderSkins.FindAsync(extraSkinA) ?? throw new InvalidOperationException());
            viewer.LeaderSkins.Add(await ctx.LeaderSkins.FindAsync(extraSkinB) ?? throw new InvalidOperationException());
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/profile/index", JsonBody("{}"));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var entry = doc.RootElement.GetProperty("user_class_list")
            .EnumerateArray()
            .First(e => e.GetProperty("class_id").GetInt32() == 1);
        var list = entry.GetProperty("leader_skin_id_list");
        var ids = Enumerable.Range(0, list.GetArrayLength()).Select(i => list[i].GetInt32()).ToList();
        Assert.That(ids, Does.Contain(extraSkinA));
        Assert.That(ids, Does.Contain(extraSkinB));
        Assert.That(ids.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task Index_is_random_leader_skin_reflects_persisted_value()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await ctx.Viewers.Include(v => v.Classes).ThenInclude(c => c.Class).FirstAsync(v => v.Id == viewerId);
            viewer.Classes.First(c => c.Class.Id == 1).IsRandomLeaderSkin = true;
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/profile/index", JsonBody("{}"));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var class1 = doc.RootElement.GetProperty("user_class_list")
            .EnumerateArray()
            .First(e => e.GetProperty("class_id").GetInt32() == 1);
        Assert.That(class1.GetProperty("is_random_leader_skin").GetInt32(), Is.EqualTo(1));

        var class2 = doc.RootElement.GetProperty("user_class_list")
            .EnumerateArray()
            .First(e => e.GetProperty("class_id").GetInt32() == 2);
        Assert.That(class2.GetProperty("is_random_leader_skin").GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public async Task Index_without_auth_returns_401()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateClient();  // no X-Test-Viewer-Id header

        var response = await client.PostAsync("/profile/index", JsonBody("{}"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
