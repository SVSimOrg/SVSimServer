using System.Net;
using System.Text;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class DegreeControllerUpdateTests
{
    [Test]
    public async Task Update_persists_selected_degree_when_owned()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.Degrees).FirstAsync(v => v.Id == viewerId);
            var d500 = await db.Set<DegreeEntry>().FirstOrDefaultAsync(d => d.Id == 500)
                       ?? db.Set<DegreeEntry>().Add(new DegreeEntry { Id = 500 }).Entity;
            viewer.Degrees.Add(d500);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var requestJson = """{"degree_id":500,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/degree/update_degree",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer2 = await db2.Viewers
            .Include(v => v.Info.SelectedDegree)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer2.Info.SelectedDegree.Id, Is.EqualTo(500));
    }

    [Test]
    public async Task Update_rejects_unowned_degree()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = """{"degree_id":99999,"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/degree/update_degree",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
