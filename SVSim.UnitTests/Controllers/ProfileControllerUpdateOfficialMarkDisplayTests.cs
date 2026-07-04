using System.Net;
using System.Text;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class ProfileControllerUpdateOfficialMarkDisplayTests
{
    [TestCase(0, false)]
    [TestCase(1, true)]
    public async Task UpdateOfficialMarkDisplay_persists_flag(int wireValue, bool expected)
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(tutorialState: 0);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var requestJson = $$"""{"is_official_mark_displayed":{{wireValue}},"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";
        var response = await client.PostAsync("/profile/update_official_mark_display",
            new StringContent(requestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers.FirstAsync(v => v.Id == viewerId);
        Assert.That(viewer.Info.IsOfficialMarkDisplayed, Is.EqualTo(expected));
    }
}
