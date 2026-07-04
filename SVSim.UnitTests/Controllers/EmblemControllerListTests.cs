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

public class EmblemControllerListTests
{
    [Test]
    public async Task EmblemList_returns_owned_ids_wrapped()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.Emblems).FirstAsync(v => v.Id == viewerId);
            var e100 = await db.Set<EmblemEntry>().FirstOrDefaultAsync(e => e.Id == 100)
                       ?? db.Set<EmblemEntry>().Add(new EmblemEntry { Id = 100 }).Entity;
            var e101 = await db.Set<EmblemEntry>().FirstOrDefaultAsync(e => e.Id == 101)
                       ?? db.Set<EmblemEntry>().Add(new EmblemEntry { Id = 101 }).Entity;
            viewer.Emblems.Add(e100);
            viewer.Emblems.Add(e101);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/emblem/emblem_list",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var ids = doc.RootElement.GetProperty("user_emblem_list").EnumerateArray()
            .Select(e => e.GetProperty("emblem_id").GetInt32())
            .ToList();
        Assert.That(ids, Does.Contain(100));
        Assert.That(ids, Does.Contain(101));
    }
}
