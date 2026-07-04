using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Coverage for <c>/check/*</c> — the first two endpoints the client hits on boot. The
/// SpecialTitle smoke is duplicated in RoutingSmokeTests for routing-prefix coverage; this
/// test layers shape assertions over the deeper boot-path concern.
/// </summary>
public class CheckControllerTests
{
    private const string BaseRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    private const string GameStartRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","app_type":0,"campaign_data":"","campaign_sign":"","campaign_user":0}""";

    [Test]
    public async Task SpecialTitle_returns_default_title_id()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/check/special_title",
            new StringContent(BaseRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.That(doc.RootElement.GetProperty("title_image_id").GetString(), Is.EqualTo("0"));
    }

    [Test]
    public async Task GameStart_with_authed_viewer_returns_spec_shape()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/check/game_start",
            new StringContent(GameStartRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // now_tutorial_step is a STRING on the wire (prod sends "100"); client calls .ToInt().
        Assert.That(root.GetProperty("now_tutorial_step").GetString(), Is.EqualTo("100"),
            "RegisterViewer's seed-config default sets tutorial_state=100 (tutorial complete).");
        Assert.That(root.GetProperty("tos_state").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("policy_state").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("kor_authority_state").GetInt32(), Is.EqualTo(0));
        Assert.That(root.GetProperty("tos_id").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("policy_id").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("kor_authority_id").GetInt32(), Is.EqualTo(0));

        // Prod-shape fields (not strictly read by GameStartCheckTask.Parse but sent by prod).
        Assert.That(root.GetProperty("now_viewer_id").GetInt64(), Is.GreaterThan(0));
        Assert.That(root.GetProperty("now_name").GetString(), Is.Not.Empty);
        Assert.That(root.GetProperty("now_rank").ValueKind, Is.EqualTo(JsonValueKind.Object));

        // Steam connection should round-trip into transition_account_data — all three fields
        // serialized as strings (matches prod wire shape).
        var transitions = root.GetProperty("transition_account_data");
        Assert.That(transitions.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(transitions.GetArrayLength(), Is.EqualTo(1),
            "Seeded viewer has exactly one Steam social account connection.");
        Assert.That(transitions[0].GetProperty("social_account_type").GetString(),
            Is.EqualTo(((int)SVSim.Database.Enums.SocialAccountType.Steam).ToString()));
        Assert.That(transitions[0].GetProperty("social_account_id").GetString(), Is.Not.Empty);
        Assert.That(transitions[0].GetProperty("connected_viewer_id").GetString(), Is.Not.Empty);
    }

    [Test]
    public async Task GameStart_does_not_expose_unsettable_optional_fields()
    {
        // GameStartCheckTask.Parse uses `Keys.Contains("rewrite_viewer_id")` + `.ToInt()` with
        // no null guard, and same for `account_delete_reservation_status` (presence-only check).
        // We can't omit nullable properties on the encrypted MessagePack path — the [Key]
        // formatter writes them as Nil unconditionally. So these keys must not exist on
        // GameStartResponse at all. If a future change re-adds them, this test breaks the build.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/check/game_start",
            new StringContent(GameStartRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.That(root.TryGetProperty("rewrite_viewer_id", out _), Is.False,
            "rewrite_viewer_id must NOT be present in the response — client NREs on null .ToInt().");
        Assert.That(root.TryGetProperty("account_delete_reservation_status", out _), Is.False,
            "account_delete_reservation_status must NOT be present — presence triggers client behavior.");
    }

    [Test]
    public async Task GameStart_emits_rewrite_viewer_id_when_udid_keyed_viewer_differs_from_authed_viewer()
    {
        // Reproduces the wipe-and-resignup scenario: the client wiped local prefs, hit
        // /tool/signup with a fresh UDID (creating a blank anonymous viewer V_new), then
        // hit /check/game_start carrying the same Steam ticket. The Steam handler resolves
        // to V_old (the original viewer with the Steam link), but the client still thinks
        // it is V_new from the signup response. rewrite_viewer_id is the documented client
        // hook for correcting Certification.ViewerId — see Cute/GameStartCheckTask.cs:113.
        using var factory = new SVSimTestFactory();
        long oldViewerId = await factory.SeedViewerAsync();

        // V_new: blank-named anonymous viewer keyed by a fresh UDID, as RegisterAnonymousViewer
        // would have produced inside /tool/signup. Resolved via the real repo so the row matches
        // what production lays down.
        Guid freshUdid = Guid.NewGuid();
        long newViewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            var v = await repo.RegisterAnonymousViewer(freshUdid);
            newViewerId = v.Id;
        }

        // SID→UDID mapping for U_new, so HttpContext.GetUdid() resolves to it inside the
        // controller (same setup MakeClientWithUdid in ToolControllerTests uses).
        const string sid = "wipe-resignup-test-sid";
        using (var scope = factory.Services.CreateScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<ShadowverseSessionService>();
            session.StoreUdidForSessionId(sid, freshUdid);
        }

        using var client = factory.CreateAuthenticatedClient(oldViewerId);
        client.DefaultRequestHeaders.Add("SID", sid);

        var response = await client.PostAsync("/check/game_start",
            new StringContent(GameStartRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.That(root.TryGetProperty("rewrite_viewer_id", out var rewrite), Is.True,
            $"rewrite_viewer_id must be present when UDID-keyed viewer ({newViewerId}) differs from auth-resolved viewer ({oldViewerId}). " +
            "Without it the client stays stuck on the wrong Certification.ViewerId after a wipe-and-resignup. Body: " + body);
        Assert.That(rewrite.GetInt64(), Is.EqualTo(oldViewerId),
            "rewrite_viewer_id must point to the auth-resolved (Steam-linked) viewer, not the UDID-keyed anonymous one.");
    }

    [Test]
    public async Task GameStart_deletes_anonymous_viewer_and_repoints_udid_on_mismatch()
    {
        // Same wipe-and-resignup scenario as the rewrite_viewer_id test, but asserting the
        // server-side cleanup: V_new (the blank anonymous viewer /tool/signup just created)
        // must be deleted, and V_old must take ownership of the fresh UDID so future
        // GetViewerByUdid lookups resolve straight to V_old without going through Steam.
        using var factory = new SVSimTestFactory();
        long oldViewerId = await factory.SeedViewerAsync();

        Guid freshUdid = Guid.NewGuid();
        long newViewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            var v = await repo.RegisterAnonymousViewer(freshUdid);
            newViewerId = v.Id;
        }

        const string sid = "wipe-resignup-cleanup-sid";
        using (var scope = factory.Services.CreateScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<ShadowverseSessionService>();
            session.StoreUdidForSessionId(sid, freshUdid);
        }

        using var client = factory.CreateAuthenticatedClient(oldViewerId);
        client.DefaultRequestHeaders.Add("SID", sid);

        var response = await client.PostAsync("/check/game_start",
            new StringContent(GameStartRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        using var scope2 = factory.Services.CreateScope();
        var db = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();

        Assert.That(await db.Viewers.AnyAsync(v => v.Id == newViewerId), Is.False,
            $"V_new (id={newViewerId}, the blank anonymous viewer from /tool/signup) must be deleted after game_start detected the Steam-vs-UDID mismatch.");

        var oldViewer = await db.Viewers.FirstOrDefaultAsync(v => v.Id == oldViewerId);
        Assert.That(oldViewer, Is.Not.Null, "V_old must still exist — only V_new should be deleted.");
        Assert.That(oldViewer!.Udid, Is.EqualTo(freshUdid),
            "V_old must take ownership of V_new's UDID so future UDID-only lookups resolve directly to V_old.");
    }

    [Test]
    public async Task GameStart_does_not_touch_viewers_when_udid_matches_authed_viewer()
    {
        // No mismatch → no cleanup. Sanity check that the merge path doesn't fire when the
        // UDID resolves to the same viewer the Steam ticket did (the normal post-signup flow
        // when no wipe happened, or a second game_start call after a wipe has already been
        // reconciled).
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        // Point both V_old and the SID map at the same UDID so HttpContext.GetUdid()
        // resolves to V_old itself.
        Guid sharedUdid = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.FirstAsync(x => x.Id == viewerId);
            v.Udid = sharedUdid;
            await db.SaveChangesAsync();
        }

        const string sid = "no-mismatch-sid";
        using (var scope = factory.Services.CreateScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<ShadowverseSessionService>();
            session.StoreUdidForSessionId(sid, sharedUdid);
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        client.DefaultRequestHeaders.Add("SID", sid);

        var response = await client.PostAsync("/check/game_start",
            new StringContent(GameStartRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db2.Viewers.FirstOrDefaultAsync(v => v.Id == viewerId);
        Assert.That(viewer, Is.Not.Null, "Viewer must not be deleted when UDID matches.");
        Assert.That(viewer!.Udid, Is.EqualTo(sharedUdid), "UDID must be untouched in the no-mismatch path.");
    }

    [Test]
    public async Task GameStart_with_no_viewer_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/check/game_start",
            new StringContent(GameStartRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
