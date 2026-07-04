using Microsoft.Extensions.DependencyInjection;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Services.Guild;
using SVSim.UnitTests.Infrastructure;
using GuildEntity = SVSim.Database.Entities.Guild.Guild;

namespace SVSim.UnitTests.Services.Guild;

public class GuildServiceCreateTests
{
    private static async Task<long> CreateViewerAsync(SVSimTestFactory factory, ulong steamId = 76_561_198_100_000_001UL, string name = "GuildTestViewer")
        => await factory.SeedViewerAsync(steamId: steamId, displayName: name);

    private static IGuildService GuildService(SVSimTestFactory factory, out IServiceScope scope)
    {
        scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IGuildService>();
    }

    [Test]
    public async Task CreateAsync_makes_a_guild_with_caller_as_leader()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await CreateViewerAsync(factory, 76_561_198_100_000_001UL, "Alpha Leader");

        GuildOpResult res;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            res = await svc.CreateAsync(viewerId, new("Alpha", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        }

        Assert.That(res.IsOk, Is.True);
        Assert.That(res.GuildId, Is.Not.Null);

        GuildFullView? view;
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            view = await svc.GetMyGuildAsync(viewerId);
        }

        Assert.That(view, Is.Not.Null);
        Assert.That(view!.Guild.Name, Is.EqualTo("Alpha"));
        Assert.That(view.Guild.LeaderViewerId, Is.EqualTo(viewerId));
        Assert.That(view.Members.Single().Role, Is.EqualTo(GuildRole.Leader));
    }

    [Test]
    public async Task CreateAsync_returns_AlreadyInGuild_when_viewer_already_in_a_guild()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await CreateViewerAsync(factory, 76_561_198_100_000_002UL, "Already In");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var first = await svc.CreateAsync(viewerId, new("FirstGuild", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(first.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var second = await svc.CreateAsync(viewerId, new("SecondGuild", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(second.Code, Is.EqualTo(GuildOpResultCode.AlreadyInGuild));
        }
    }

    [Test]
    public async Task CreateAsync_returns_NameTaken_when_guild_name_already_exists()
    {
        using var factory = new SVSimTestFactory();
        var v1 = await CreateViewerAsync(factory, 76_561_198_100_000_003UL, "Viewer1");
        var v2 = await CreateViewerAsync(factory, 76_561_198_100_000_004UL, "Viewer2");

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var first = await svc.CreateAsync(v1, new("SameName", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(first.IsOk, Is.True);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
            var second = await svc.CreateAsync(v2, new("SameName", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
            Assert.That(second.Code, Is.EqualTo(GuildOpResultCode.NameTaken));
        }
    }

    [Test]
    public async Task CreateAsync_returns_NameInvalid_for_empty_name()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await CreateViewerAsync(factory, 76_561_198_100_000_005UL);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
        var res = await svc.CreateAsync(viewerId, new("", (int)GuildActivity.All, (int)GuildJoinCondition.Free));
        Assert.That(res.Code, Is.EqualTo(GuildOpResultCode.NameInvalid));
    }

    [Test]
    public async Task GetMyGuildAsync_returns_null_for_viewer_not_in_any_guild()
    {
        using var factory = new SVSimTestFactory();
        var viewerId = await CreateViewerAsync(factory, 76_561_198_100_000_006UL);

        using var scope = factory.Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IGuildService>();
        var view = await svc.GetMyGuildAsync(viewerId);
        Assert.That(view, Is.Null);
    }
}
