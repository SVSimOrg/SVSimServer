using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class FriendControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    // Minimal BaseRequest-shaped payload. Body-less /friend/* actions now declare
    // [FromBody] BaseRequest _ so the prod translation middleware can deserialize
    // the encrypted msgpack body (it requires at least one parameter). Tests post
    // these auth fields so [ApiController] model validation passes — the actual
    // viewer_id comes from the session claim, not the body.
    private const string EmptyBody = """{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""";

    private static async Task<long> SeedViewer(SVSimTestFactory factory, ulong steamId, string name = "Test Viewer")
        => await factory.SeedViewerAsync(steamId: steamId, displayName: name);

    [Test]
    public async Task FriendInfo_returns_empty_friends_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_010_001UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/friend/info", JsonBody(EmptyBody));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("friends").GetArrayLength(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("friend_count").GetInt32(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("friend_max_count").GetInt32(), Is.EqualTo(110));
    }

    [Test]
    public async Task ReceiveApplyInfo_returns_empty_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_010_002UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/friend/receive_apply_info", JsonBody(EmptyBody));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("receive_applies").GetArrayLength(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("approve_apply_count").GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public async Task SendApplyInfo_returns_empty_with_full_remaining_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_010_003UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/friend/send_apply_info", JsonBody(EmptyBody));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("send_applies").GetArrayLength(), Is.EqualTo(0));
        Assert.That(doc.RootElement.GetProperty("remaining_apply_count").GetInt32(), Is.EqualTo(110));
        Assert.That(doc.RootElement.GetProperty("send_apply_max_count").GetInt32(), Is.EqualTo(110));
    }

    [Test]
    public async Task PlayedTogetherInfo_returns_empty_histories()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_010_004UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/friend/played_together_info", JsonBody(EmptyBody));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("histories").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task SearchUser_returns_empty_object_for_unknown_id()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await SeedViewer(factory, 76_561_198_000_010_005UL);
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/friend/search_user", JsonBody("""{"search_viewer_id":999999}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var userInfo = doc.RootElement.GetProperty("user_info");
        Assert.That(userInfo.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(userInfo.EnumerateObject().Count(), Is.EqualTo(0), "no match → {}");
    }

    [Test]
    public async Task SearchUser_returns_populated_user_info_for_existing_viewer()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_006UL);
        long target = await SeedViewer(factory, 76_561_198_000_010_007UL, "Target");
        using var client = factory.CreateAuthenticatedClient(me);

        var response = await client.PostAsync("/friend/search_user", JsonBody($$"""{"search_viewer_id":{{(int)target}}}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        var userInfo = doc.RootElement.GetProperty("user_info");
        Assert.That(userInfo.GetProperty("name").GetString(), Is.EqualTo("Target"));
        Assert.That(userInfo.GetProperty("viewer_id").GetInt32(), Is.EqualTo((int)target));
    }

    [Test]
    public async Task SendApply_persists_apply_row()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_008UL);
        long target = await SeedViewer(factory, 76_561_198_000_010_009UL);
        using var client = factory.CreateAuthenticatedClient(me);

        var response = await client.PostAsync("/friend/send_apply", JsonBody($$"""{"friend_id":{{(int)target}}}"""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx.ViewerFriendApplies.CountAsync(a => a.FromViewerId == me && a.ToViewerId == target), Is.EqualTo(1));
    }

    [Test]
    public async Task ApproveApply_creates_friendship()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_010UL);
        long sender = await SeedViewer(factory, 76_561_198_000_010_011UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = me, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        using var client = factory.CreateAuthenticatedClient(me);
        var response = await client.PostAsync("/friend/approve_apply", JsonBody($$"""{"apply_id":{{applyId}}}"""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(await ctx2.ViewerFriends.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task RejectApply_deletes_incoming_apply()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_012UL);
        long sender = await SeedViewer(factory, 76_561_198_000_010_013UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var apply = new ViewerFriendApply { FromViewerId = sender, ToViewerId = me, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        using var client = factory.CreateAuthenticatedClient(me);
        var response = await client.PostAsync("/friend/reject_apply", JsonBody($$"""{"apply_id":{{applyId}}}"""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>().ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task CancelApply_deletes_outgoing_apply()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_014UL);
        long target = await SeedViewer(factory, 76_561_198_000_010_015UL);

        int applyId;
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var apply = new ViewerFriendApply { FromViewerId = me, ToViewerId = target, CreatedAt = DateTime.UtcNow };
            ctx.ViewerFriendApplies.Add(apply);
            await ctx.SaveChangesAsync();
            applyId = apply.Id;
        }

        using var client = factory.CreateAuthenticatedClient(me);
        var response = await client.PostAsync("/friend/cancel_apply", JsonBody($$"""{"apply_id":{{applyId}}}"""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>().ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RejectAllApplies_clears_incoming()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_016UL);
        long sender = await SeedViewer(factory, 76_561_198_000_010_017UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = sender, ToViewerId = me, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(me);
        var response = await client.PostAsync("/friend/reject_apply_all", JsonBody(EmptyBody));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>().ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task CancelAllApplies_clears_outgoing()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_018UL);
        long target = await SeedViewer(factory, 76_561_198_000_010_019UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerFriendApplies.Add(new ViewerFriendApply { FromViewerId = me, ToViewerId = target, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(me);
        var response = await client.PostAsync("/friend/cancel_apply_all", JsonBody(EmptyBody));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>().ViewerFriendApplies.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RejectFriend_removes_both_friendship_rows()
    {
        using var factory = new SVSimTestFactory();
        long me = await SeedViewer(factory, 76_561_198_000_010_020UL);
        long friend = await SeedViewer(factory, 76_561_198_000_010_021UL);

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = me, FriendViewerId = friend, CreatedAt = DateTime.UtcNow });
            ctx.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = friend, FriendViewerId = me, CreatedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(me);
        var response = await client.PostAsync("/friend/reject_friend", JsonBody($$"""{"friend_id":{{(int)friend}}}"""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = factory.Services.CreateScope();
        Assert.That(await verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>().ViewerFriends.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task FriendInfo_without_auth_returns_401()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsync("/friend/info", JsonBody(EmptyBody));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Multi_viewer_flow_A_sends_B_approves_both_see_friend()
    {
        using var factory = new SVSimTestFactory();
        long viewerA = await SeedViewer(factory, 76_561_198_000_020_001UL, "Alice");
        long viewerB = await SeedViewer(factory, 76_561_198_000_020_002UL, "Bob");

        // A sends apply to B.
        using (var clientA = factory.CreateAuthenticatedClient(viewerA))
        {
            var resp = await clientA.PostAsync("/friend/send_apply", JsonBody($$"""{"friend_id":{{(int)viewerB}}}"""));
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        // B sees the apply in receive_apply_info.
        int applyId;
        using (var clientB = factory.CreateAuthenticatedClient(viewerB))
        {
            var resp = await clientB.PostAsync("/friend/receive_apply_info", JsonBody(EmptyBody));
            var raw = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(raw);
            var applies = doc.RootElement.GetProperty("receive_applies");
            Assert.That(applies.GetArrayLength(), Is.EqualTo(1));
            applyId = applies[0].GetProperty("id").GetInt32();
            Assert.That(applies[0].GetProperty("name").GetString(), Is.EqualTo("Alice"));
        }

        // B approves.
        using (var clientB = factory.CreateAuthenticatedClient(viewerB))
        {
            var resp = await clientB.PostAsync("/friend/approve_apply", JsonBody($$"""{"apply_id":{{applyId}}}"""));
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        // Both A and B now see each other in /friend/info.
        async Task<string> GetFriendName(long ownerId)
        {
            using var client = factory.CreateAuthenticatedClient(ownerId);
            var resp = await client.PostAsync("/friend/info", JsonBody(EmptyBody));
            var raw = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(raw);
            var friends = doc.RootElement.GetProperty("friends");
            Assert.That(friends.GetArrayLength(), Is.EqualTo(1));
            return friends[0].GetProperty("name").GetString()!;
        }

        Assert.That(await GetFriendName(viewerA), Is.EqualTo("Bob"));
        Assert.That(await GetFriendName(viewerB), Is.EqualTo("Alice"));
    }
}
