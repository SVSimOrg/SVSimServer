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

public class DegreeControllerListTests
{
    [Test]
    public async Task DegreeList_returns_owned_ids_wrapped()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.Degrees).FirstAsync(v => v.Id == viewerId);
            var d100 = await db.Set<DegreeEntry>().FirstOrDefaultAsync(d => d.Id == 100)
                       ?? db.Set<DegreeEntry>().Add(new DegreeEntry { Id = 100 }).Entity;
            var d101 = await db.Set<DegreeEntry>().FirstOrDefaultAsync(d => d.Id == 101)
                       ?? db.Set<DegreeEntry>().Add(new DegreeEntry { Id = 101 }).Entity;
            viewer.Degrees.Add(d100);
            viewer.Degrees.Add(d101);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = """{"degree_id":0,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/degree/degree_list",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var ids = doc.RootElement.GetProperty("user_degree_list").EnumerateArray()
            .Select(e => e.GetProperty("degree_id").GetInt32())
            .ToList();
        Assert.That(ids, Does.Contain(100));
        Assert.That(ids, Does.Contain(101));
    }
}
