using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Coverage for <c>/mypage/index</c>. Focused on fields computed from viewer state
/// that are easy to regress (can_give_daily_login_bonus).
/// </summary>
public class MyPageControllerTests
{
    // MyPageIndexRequest extends BaseRequest, so viewer_id + steam_session_ticket are required.
    private const string MyPageRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"steam"}""";

    private const string LoadIndexRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"steam","card_master_hash":""}""";

    [Test]
    public async Task MyPage_can_give_daily_login_bonus_is_true_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/mypage/index",
            new StringContent(MyPageRequestJson, Encoding.UTF8, "application/json"));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        Assert.That(root.GetProperty("can_give_daily_login_bonus").GetBoolean(), Is.True);
    }

    [Test]
    public async Task MyPage_can_give_daily_login_bonus_is_false_after_load_index_claim()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        // /load/index claims today's bonus
        var loadResp = await client.PostAsync("/load/index",
            new StringContent(LoadIndexRequestJson, Encoding.UTF8, "application/json"));
        Assert.That(loadResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await loadResp.Content.ReadAsStringAsync());

        // /mypage/index must report flag = false after the claim
        var resp = await client.PostAsync("/mypage/index",
            new StringContent(MyPageRequestJson, Encoding.UTF8, "application/json"));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        Assert.That(root.GetProperty("can_give_daily_login_bonus").GetBoolean(), Is.False);
    }

    [Test]
    public async Task MyPage_unread_present_count_reflects_unclaimed_viewer_presents()
    {
        // Drives the home-screen crate badge — MyPageTask parses `unread_present_count` into
        // Data.MyPage.data.unread_mail_count, which MyPageItemHome.SetUnreadGiftCount reads to
        // show the "N" bubble on the gift button. Stubbed to 0 would hide it even when the
        // tutorial gift has 5 rewards sitting unclaimed.
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedTutorialPresentsAsync(viewerId);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var resp = await client.PostAsync("/mypage/index",
            new StringContent(MyPageRequestJson, Encoding.UTF8, "application/json"));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), await resp.Content.ReadAsStringAsync());

        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        int count = root.GetProperty("unread_present_count").GetInt32();
        Assert.That(count, Is.GreaterThan(0),
            "unread_present_count must be the viewer's live Unclaimed ViewerPresent count — a stub 0 hides the crate badge.");
    }
}
