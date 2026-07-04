using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Coverage for <c>POST /tool/signup</c> — the very first request a fresh client makes on boot.
/// Spec: <c>docs/api-spec/endpoints/pre-login/tool-signup.md</c>.
/// </summary>
public class ToolControllerTests
{
    private const string SignupBodyJson =
        """{"device_name":"DESKTOP-ABC","client_type":"PC","os_version":"Windows 10","app_version":"2.4.0","resource_version":"00000000","carrier":""}""";

    private static (HttpClient client, Guid udid) MakeClientWithUdid(SVSimTestFactory factory, string sid = "test-sid")
    {
        var udid = Guid.NewGuid();
        // SessionidMappingMiddleware needs both headers OR we can populate the session service
        // directly. Populate directly so we don't have to model the encrypted-UDID header
        // here (Encryption.Decode runs on the encoded header). A non-empty SID is required —
        // HttpClient strips empty header values, so Request.Headers["SID"] would resolve to
        // null and GetUdid() would return null (collapsing into the same path as the
        // no-mapping case below).
        using (var scope = factory.Services.CreateScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<ShadowverseSessionService>();
            session.StoreUdidForSessionId(sid, udid);
        }
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("SID", sid);
        return (client, udid);
    }

    [Test]
    public async Task Signup_creates_viewer_with_udid_and_returns_200()
    {
        using var factory = new SVSimTestFactory();
        var (client, udid) = MakeClientWithUdid(factory);

        var response = await client.PostAsync("/tool/signup",
            new StringContent(SignupBodyJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(v => v.Classes)
            .Include(v => v.SocialAccountConnections)
            .FirstOrDefaultAsync(v => v.Udid == udid);

        Assert.That(viewer, Is.Not.Null, "Signup should have persisted a Viewer keyed on the request UDID.");
        Assert.That(viewer!.SocialAccountConnections, Is.Empty,
            "Anonymous signup must not pre-link a social account.");
        Assert.That(viewer.Classes, Is.Not.Empty,
            "Default-loadout body should populate Classes (BuildDefaultViewer wiring).");
    }

    [Test]
    public async Task Signup_is_idempotent_by_udid()
    {
        using var factory = new SVSimTestFactory();
        var (client, udid) = MakeClientWithUdid(factory);

        var r1 = await client.PostAsync("/tool/signup",
            new StringContent(SignupBodyJson, Encoding.UTF8, "application/json"));
        var r2 = await client.PostAsync("/tool/signup",
            new StringContent(SignupBodyJson, Encoding.UTF8, "application/json"));

        Assert.That(r1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(r2.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var matches = await db.Viewers.Where(v => v.Udid == udid).CountAsync();
        Assert.That(matches, Is.EqualTo(1),
            "Second signup must reuse the existing Viewer row, not create a duplicate.");
    }

    [Test]
    public async Task Signup_with_no_udid_returns_5xx()
    {
        // No SID→UDID mapping installed; the controller should refuse rather than create a
        // ghost-Empty viewer (which would dedup all broken clients into a single row).
        // In production this surfaces as 500 (Kestrel converts unhandled exceptions). The
        // TestHost ClientHandler rethrows by default, so we accept either: a 5xx response, or
        // the InvalidOperationException propagating out of HttpClient.PostAsync.
        using var factory = new SVSimTestFactory();
        var client = factory.CreateClient();

        Exception? thrown = null;
        HttpResponseMessage? response = null;
        try
        {
            response = await client.PostAsync("/tool/signup",
                new StringContent(SignupBodyJson, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        if (thrown is null)
        {
            Assert.That(response, Is.Not.Null);
            Assert.That((int)response!.StatusCode, Is.GreaterThanOrEqualTo(500),
                "Empty/missing UDID is unrecoverable; controller should throw, not 200.");
        }
        else
        {
            Assert.That(thrown, Is.InstanceOf<InvalidOperationException>().Or.InnerException.InstanceOf<InvalidOperationException>(),
                $"Expected InvalidOperationException from controller, got {thrown.GetType().Name}: {thrown.Message}");
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await db.Viewers.AnyAsync(v => v.Udid == Guid.Empty), Is.False,
            "No ghost-Empty viewer should have been written.");
    }
}
