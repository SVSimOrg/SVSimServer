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

public class AchievementControllerTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task ImportCatalogs(SVSimTestFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await new MissionCatalogImporter().ImportAsync(db, SeedDir);
        await new AchievementCatalogImporter().ImportAsync(db, SeedDir);
        await new BattlePassMonthlyMissionImporter().ImportAsync(db, SeedDir);
    }

    private static async Task<ulong> RupeesBalance(SVSimTestFactory f, long viewerId)
    {
        await using var scope = f.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await db.Viewers.Include(x => x.Currency).FirstAsync(x => x.Id == viewerId);
        return v.Currency.Rupees;
    }

    [Test]
    public async Task Claim_advances_level_by_one()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        // Materialize achievements at MIN(Level) per type
        await client.PostAsync("/mission/info", JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        // Find an achievement that the viewer is at and claim it.
        // Type 50 ("Achieve Beginner 3 rank") has captured tier at level 3 only — viewer starts at 3.
        var claimBody = """{"achievement_type":50,"level":3,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/achievement/receive_reward", JsonBody(claimBody));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var refreshed = await db.ViewerAchievements
            .FirstAsync(a => a.ViewerId == viewerId && a.AchievementType == 50);
        Assert.That(refreshed.Level, Is.EqualTo(4));
    }

    [Test]
    public async Task Claim_grants_rupees_via_RewardGrantService()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        await client.PostAsync("/mission/info", JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        ulong rupeesBefore = await RupeesBalance(factory, viewerId);

        // Type 50 reward in capture: reward_type=9 (Rupy in UserGoodsType enum), reward_number=100.
        var claimBody = """{"achievement_type":50,"level":3,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/achievement/receive_reward", JsonBody(claimBody));
        resp.EnsureSuccessStatusCode();

        ulong rupeesAfter = await RupeesBalance(factory, viewerId);
        Assert.That(rupeesAfter, Is.EqualTo(rupeesBefore + 100UL),
            "claiming type 50 / level 3 should grant 100 rupees");
    }

    [Test]
    public async Task Claim_response_contains_reward_list()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        await client.PostAsync("/mission/info", JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var claimBody = """{"achievement_type":50,"level":3,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var resp = await client.PostAsync("/achievement/receive_reward", JsonBody(claimBody));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        var rewardList = doc.RootElement.GetProperty("reward_list");
        Assert.That(rewardList.GetArrayLength(), Is.GreaterThanOrEqualTo(1));
        var grant = rewardList[0];
        Assert.That(grant.GetProperty("reward_type").GetInt32(), Is.EqualTo(9));
        // For currency grants, reward_num is the POST-STATE TOTAL (per project convention,
        // matches /pack/open behavior). Default-seeded viewer starts at 0 rupees → 100 after grant.
        Assert.That(grant.GetProperty("reward_num").GetInt32(), Is.GreaterThanOrEqualTo(100),
            "reward_num is post-state total for currencies, must be at least the granted amount");
    }

    [Test]
    public async Task Subsequent_claim_of_same_level_fails()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await ImportCatalogs(factory);

        using var client = factory.CreateAuthenticatedClient(viewerId);
        await client.PostAsync("/mission/info", JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));

        var claimBody = """{"achievement_type":50,"level":3,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

        var first = await client.PostAsync("/achievement/receive_reward", JsonBody(claimBody));
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsync("/achievement/receive_reward", JsonBody(claimBody));
        var body = await second.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("\"result_code\":2"),
            "second claim at the same level must fail with result_code=2");
    }
}
