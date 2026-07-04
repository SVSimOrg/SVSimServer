using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.EmulatedEntrypoint;

namespace SVSim.UnitTests.Story;

/// <summary>
/// Smoke tests for the 21 story URLs. We assert the framework matched the route
/// (status != 404). Auth-required routes return 401, which is fine — that still means routing matched.
/// </summary>
[TestFixture]
public class RoutingSmokeTestsStory
{
    private sealed class TestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SVSimDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<SVSimDbContext>(opt => opt.UseInMemoryDatabase("RoutingSmokeStory"));
            });
        }
    }

    private const string ValidBaseRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    [TestCase("/story/section")]
    [TestCase("/main_story/section")]
    [TestCase("/limited_story/section")]
    [TestCase("/event_story/section")]
    [TestCase("/main_story/leader_select")]
    [TestCase("/limited_story/leader_select")]
    [TestCase("/event_story/leader_select")]
    [TestCase("/main_story/info")]
    [TestCase("/limited_story/info")]
    [TestCase("/event_story/info")]
    [TestCase("/main_story/get_deck_list")]
    [TestCase("/event_story/get_deck_list")]
    [TestCase("/main_story/start")]
    [TestCase("/limited_story/start")]
    [TestCase("/event_story/start")]
    [TestCase("/main_story/finish")]
    [TestCase("/limited_story/finish")]
    [TestCase("/event_story/finish")]
    [TestCase("/main_story/all_finish")]
    [TestCase("/limited_story/all_finish")]
    [TestCase("/event_story/all_finish")]
    public async Task Story_route_resolves(string path)
    {
        using var factory = new TestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(path,
            new StringContent(ValidBaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound),
            $"Route {path} did not match — route registration broken.");
    }
}
