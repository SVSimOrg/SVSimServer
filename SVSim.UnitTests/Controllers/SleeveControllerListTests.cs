using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class SleeveControllerListTests
{
    [Test]
    public async Task SleeveList_returns_owned_ids_wrapped()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);

        // Grant two sleeves directly. (Avoid the buy path — we want the test to
        // only exercise the read.)
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.Sleeves).FirstAsync(v => v.Id == viewerId);
            // SleeveEntry is a globally-shared catalog row; reuse two ids that
            // can plausibly exist or get created on the fly.
            var s100 = await db.Set<SleeveEntry>().FirstOrDefaultAsync(s => s.Id == 100)
                       ?? db.Set<SleeveEntry>().Add(new SleeveEntry { Id = 100 }).Entity;
            var s101 = await db.Set<SleeveEntry>().FirstOrDefaultAsync(s => s.Id == 101)
                       ?? db.Set<SleeveEntry>().Add(new SleeveEntry { Id = 101 }).Entity;
            viewer.Sleeves.Add(s100);
            viewer.Sleeves.Add(s101);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/sleeve/sleeve_list",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var ids = doc.RootElement.EnumerateArray()
            .Select(e => e.GetProperty("sleeve_id").GetInt64())
            .ToHashSet();
        // Viewer has a default sleeve from RegisterViewer; just assert our two grants are present.
        Assert.That(ids, Does.Contain(100L));
        Assert.That(ids, Does.Contain(101L));
    }
}
