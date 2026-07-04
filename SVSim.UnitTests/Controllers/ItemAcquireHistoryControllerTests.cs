using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ItemAcquireHistoryControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    [Test]
    public async Task Info_returns_history_in_newest_first_order_for_the_authenticated_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_001UL);
        long viewerB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_002UL);

        using var seedScope = factory.Services.CreateScope();
        var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var baseTime = new DateTime(2026, 6, 9, 12, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 5; i++)
        {
            ctx.ViewerAcquireHistory.Add(new ViewerAcquireHistoryEntry
            {
                ViewerId = viewerA,
                RewardType = 9,
                RewardDetailId = 0,
                RewardCount = i + 1,
                AcquireType = (int)GrantSource.DailyBonus,
                Message = "Daily Bonus",
                AcquireTime = baseTime.AddMinutes(i),
            });
        }
        ctx.ViewerAcquireHistory.Add(new ViewerAcquireHistoryEntry
        {
            ViewerId = viewerB,
            RewardType = 9, RewardDetailId = 0, RewardCount = 99,
            AcquireType = (int)GrantSource.PackOpen, Message = "x", AcquireTime = baseTime,
        });
        await ctx.SaveChangesAsync();

        using var client = factory.CreateAuthenticatedClient(viewerA);
        var response = await client.PostAsync("/item_acquire_history/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        // Tests bypass the translation middleware (no UnityPlayer UA), so the response is
        // the raw controller JSON — no {data_headers, data} envelope.
        using var doc = JsonDocument.Parse(raw);
        var histories = doc.RootElement.GetProperty("histories");

        Assert.That(histories.GetArrayLength(), Is.EqualTo(5));
        // Newest first: i=4 was AcquireTime+4min → reward_count = "5"
        Assert.That(histories[0].GetProperty("reward_count").GetString(), Is.EqualTo("5"));
        Assert.That(histories[4].GetProperty("reward_count").GetString(), Is.EqualTo("1"));
        for (int i = 0; i < 5; i++)
        {
            Assert.That(histories[i].GetProperty("acquire_type").GetString(),
                Is.EqualTo("1"), $"histories[{i}].acquire_type should be DailyBonus=1");
        }
    }

    [Test]
    public async Task Info_returns_empty_array_for_viewer_with_no_history()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/item_acquire_history/info",
            JsonBody("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var histories = doc.RootElement.GetProperty("histories");
        Assert.That(histories.GetArrayLength(), Is.EqualTo(0));
    }
}
