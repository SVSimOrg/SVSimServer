using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

/// <summary>
/// Direct tests against <see cref="ViewerRepository"/>. The owned-type lookup in
/// <see cref="ViewerRepository.GetViewerBySocialConnection"/> previously used
/// <c>_dbContext.Set&lt;SocialAccountConnection&gt;()</c> which EF couldn't translate (owned
/// types aren't queryable as a root). This test would have caught the regression.
/// </summary>
public class ViewerRepositoryTests
{
    [Test]
    public async Task GetViewerBySocialConnection_returns_viewer_when_steam_id_matches()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_111_222_333UL;

        long expectedViewerId = await factory.SeedViewerAsync(steamId: steamId, displayName: "Owner");

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();

        Viewer? found = await repo.GetViewerBySocialConnection(SocialAccountType.Steam, steamId);

        Assert.That(found, Is.Not.Null, "Expected to find the seeded viewer by Steam social connection.");
        Assert.That(found!.Id, Is.EqualTo(expectedViewerId));
        Assert.That(found.DisplayName, Is.EqualTo("Owner"));
    }

    [Test]
    public async Task GetViewerBySocialConnection_returns_null_when_steam_id_does_not_match()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedViewerAsync(steamId: 76_561_198_111_222_333UL);

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();

        Viewer? found = await repo.GetViewerBySocialConnection(SocialAccountType.Steam, 76_561_198_999_999_999UL);

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task RegisterViewer_grants_default_leader_skins_to_classes()
    {
        // Guards the just-fixed nav-graph NRE — RegisterViewer iterates ClassEntry.LeaderSkins
        // and needs the .Include to populate them. If the include is lost, this throws inside
        // SeedViewerAsync rather than reaching the assertion.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        Viewer viewer = await db.Viewers
            .Include(v => v.Classes).ThenInclude(c => c.LeaderSkin)
            .Include(v => v.LeaderSkins)
            .FirstAsync(v => v.Id == viewerId);

        Assert.That(viewer.Classes, Is.Not.Empty, "RegisterViewer should populate Classes from seed data.");
        Assert.That(viewer.Classes.Select(c => c.LeaderSkin).All(s => s is not null), Is.True,
            "Every class should have a LeaderSkin assigned (placeholder or real).");
        Assert.That(viewer.LeaderSkins, Is.Not.Empty,
            "Viewer should own at least one leader skin from class defaults.");
    }

    [Test]
    public async Task RegisterAnonymousViewer_creates_viewer_with_udid_and_no_socials()
    {
        using var factory = new SVSimTestFactory();
        var udid = Guid.NewGuid();

        long viewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            var v = await repo.RegisterAnonymousViewer(udid);
            viewerId = v.Id;
        }

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var loaded = await db.Viewers
            .Include(v => v.Classes)
            .Include(v => v.SocialAccountConnections)
            .FirstAsync(v => v.Id == viewerId);

        Assert.That(loaded.Udid, Is.EqualTo(udid));
        Assert.That(loaded.SocialAccountConnections, Is.Empty);
        Assert.That(loaded.Classes, Is.Not.Empty,
            "Default-loadout body should populate Classes (smoke-test the shared BuildDefaultViewer helper).");
    }

    [Test]
    public async Task RegisterAnonymousViewer_concurrent_same_udid_returns_existing_not_duplicate()
    {
        using var factory = new SVSimTestFactory();
        var udid = Guid.NewGuid();

        long firstId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            firstId = (await repo.RegisterAnonymousViewer(udid)).Id;
        }

        // Second register with the same UDID — simulates the race losing thread after the unique
        // index has caught the duplicate. We expect a clean re-fetch, NOT an exception or duplicate.
        long secondId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            secondId = (await repo.RegisterAnonymousViewer(udid)).Id;
        }

        Assert.That(secondId, Is.EqualTo(firstId),
            "Second register must return the existing row, not create a duplicate.");

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var count = await db.Viewers.CountAsync(v => v.Udid == udid);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task RegisterAnonymousViewer_with_empty_udid_throws()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await repo.RegisterAnonymousViewer(Guid.Empty));
    }

    [Test]
    public async Task RegisterAnonymousViewer_default_starts_at_tutorial_step_1()
    {
        using var factory = new SVSimTestFactory();
        var udid = Guid.NewGuid();

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
        var viewer = await repo.RegisterAnonymousViewer(udid);

        Assert.That(viewer.MissionData.TutorialState, Is.EqualTo(1),
            "Fresh signups default to TUTORIAL_STEP0 (=1) so the client walks through the " +
            "real tutorial. SkipTutorialConfig can toggle this to 100 (post-tutorial) for " +
            "dev / two-client-smoke fast paths.");
    }

    [Test]
    public async Task RegisterAnonymousViewer_with_SkipTutorial_enabled_starts_at_state_100()
    {
        using var factory = new SVSimTestFactory();
        await factory.EnableSkipTutorialAsync();
        var udid = Guid.NewGuid();

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
        var viewer = await repo.RegisterAnonymousViewer(udid);

        Assert.That(viewer.MissionData.TutorialState, Is.EqualTo(100),
            "When SkipTutorialConfig.Enabled is true, fresh signups land at the post-tutorial " +
            "baseline (=100) — cuts tutorial-walk-through time in the two-client PVP smoke.");
    }

    [Test]
    public async Task RegisterViewer_starts_at_post_tutorial_state()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();

        var viewer = await repo.RegisterViewer(
            "Imported Viewer",
            SVSim.Database.Enums.SocialAccountType.Steam,
            socialAccountIdentifier: 76_561_198_000_000_999UL);

        Assert.That(viewer.MissionData.TutorialState, Is.EqualTo(100),
            "RegisterViewer (admin-import + Steam-social signup) must produce a post-tutorial " +
            "viewer by default. Import requests can override via request.TutorialState; absence " +
            "means 'a prod-replica viewer ready for the home screen', NOT 'replay tutorial'.");
    }

    [Test]
    public async Task GetViewerByUdid_returns_viewer_or_null()
    {
        using var factory = new SVSimTestFactory();
        var udid = Guid.NewGuid();

        long createdId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            createdId = (await repo.RegisterAnonymousViewer(udid)).Id;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            var hit = await repo.GetViewerByUdid(udid);
            var miss = await repo.GetViewerByUdid(Guid.NewGuid());

            Assert.That(hit, Is.Not.Null);
            Assert.That(hit!.Id, Is.EqualTo(createdId));
            Assert.That(miss, Is.Null);
        }
    }

    [Test]
    public async Task LinkSteamToViewer_appends_steam_social_connection()
    {
        using var factory = new SVSimTestFactory();
        var udid = Guid.NewGuid();
        const ulong steamId = 76_561_198_900_000_001UL;

        long viewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            viewerId = (await repo.RegisterAnonymousViewer(udid)).Id;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            await repo.LinkSteamToViewer(viewerId, steamId);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
            await repo.LinkSteamToViewer(viewerId, steamId);  // second call must be a no-op
        }

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var loaded = await db.Viewers
            .Include(v => v.SocialAccountConnections)
            .FirstAsync(v => v.Id == viewerId);

        // Count == 1 proves both the initial append AND that the second LinkSteamToViewer call
        // hit the `alreadyLinked` short-circuit (idempotent re-link).
        Assert.That(loaded.SocialAccountConnections, Has.Count.EqualTo(1));
        Assert.That(loaded.SocialAccountConnections[0].AccountType,
            Is.EqualTo(SocialAccountType.Steam));
        Assert.That(loaded.SocialAccountConnections[0].AccountId, Is.EqualTo(steamId));
    }
}
