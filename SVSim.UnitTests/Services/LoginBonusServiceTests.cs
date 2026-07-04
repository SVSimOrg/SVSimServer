using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class LoginBonusServiceTests
{
    private static async Task<(SVSimTestFactory factory, long viewerId)> SetupAsync()
    {
        var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();
        return (factory, viewerId);
    }

    private static async Task<Viewer> ReloadViewerAsync(SVSimTestFactory f, long viewerId)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        return await db.Viewers.Include(v => v.Currency).FirstAsync(v => v.Id == viewerId);
    }

    [Test]
    public async Task FirstClaim_returns_dto_with_now_count_1()
    {
        var (factory, viewerId) = await SetupAsync();
        using var _ = factory;
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var bonus = scope.ServiceProvider.GetRequiredService<ILoginBonusService>();

        await using var tx = await inv.BeginAsync(viewerId);
        var dto = await bonus.GrantIfDueAsync(tx);
        await tx.CommitAsync();

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Normal, Is.Not.Null);
        Assert.That(dto.Normal!.NowCount, Is.EqualTo(1));
        Assert.That(dto.Normal.Rewards, Has.Count.EqualTo(15));
        Assert.That(dto.Total, Is.Null);
        Assert.That(dto.Campaign, Is.Empty);
    }

    [Test]
    public async Task SecondClaim_same_day_returns_null()
    {
        var (factory, viewerId) = await SetupAsync();
        using var _ = factory;

        using (var scope1 = factory.Services.CreateScope())
        {
            var inv1 = scope1.ServiceProvider.GetRequiredService<IInventoryService>();
            var bonus1 = scope1.ServiceProvider.GetRequiredService<ILoginBonusService>();
            await using var tx1 = await inv1.BeginAsync(viewerId);
            await bonus1.GrantIfDueAsync(tx1);
            await tx1.CommitAsync();
        }

        using var scope2 = factory.Services.CreateScope();
        var inv2 = scope2.ServiceProvider.GetRequiredService<IInventoryService>();
        var bonus2 = scope2.ServiceProvider.GetRequiredService<ILoginBonusService>();
        await using var tx2 = await inv2.BeginAsync(viewerId);
        var dto = await bonus2.GrantIfDueAsync(tx2);

        Assert.That(dto, Is.Null);
    }

    [Test]
    public async Task Claim_after_rollover_advances_streak()
    {
        var (factory, viewerId) = await SetupAsync();
        using var _ = factory;

        using (var s1 = factory.Services.CreateScope())
        {
            var inv = s1.ServiceProvider.GetRequiredService<IInventoryService>();
            var bonus = s1.ServiceProvider.GetRequiredService<ILoginBonusService>();
            await using var tx = await inv.BeginAsync(viewerId);
            await bonus.GrantIfDueAsync(tx);
            await tx.CommitAsync();
        }

        // Backdate the claim timestamp by 2 days to simulate a day rollover
        using (var s = factory.Services.CreateScope())
        {
            var db = s.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.FirstAsync(v => v.Id == viewerId);
            v.LastLoginBonusClaimedAt = DateTime.UtcNow.AddDays(-2);
            await db.SaveChangesAsync();
        }

        using var s2 = factory.Services.CreateScope();
        var inv2 = s2.ServiceProvider.GetRequiredService<IInventoryService>();
        var bonus2 = s2.ServiceProvider.GetRequiredService<ILoginBonusService>();
        await using var tx2 = await inv2.BeginAsync(viewerId);
        var dto = await bonus2.GrantIfDueAsync(tx2);
        await tx2.CommitAsync();

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Normal!.NowCount, Is.EqualTo(2));
        var v2 = await ReloadViewerAsync(factory, viewerId);
        Assert.That(v2.LoginBonusStreak, Is.EqualTo(2));
    }

    [Test]
    public async Task Streak_wraps_after_cycle_length()
    {
        var (factory, viewerId) = await SetupAsync();
        using var _ = factory;

        // Force the viewer into "just claimed day 15 yesterday"
        using (var s = factory.Services.CreateScope())
        {
            var db = s.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.FirstAsync(v => v.Id == viewerId);
            v.LoginBonusStreak = 15;
            v.LastLoginBonusClaimedAt = DateTime.UtcNow.AddDays(-1);
            await db.SaveChangesAsync();
        }

        using var s2 = factory.Services.CreateScope();
        var inv = s2.ServiceProvider.GetRequiredService<IInventoryService>();
        var bonus = s2.ServiceProvider.GetRequiredService<ILoginBonusService>();
        await using var tx = await inv.BeginAsync(viewerId);
        var dto = await bonus.GrantIfDueAsync(tx);
        await tx.CommitAsync();

        Assert.That(dto!.Normal!.NowCount, Is.EqualTo(1));
        var v2 = await ReloadViewerAsync(factory, viewerId);
        Assert.That(v2.LoginBonusStreak, Is.EqualTo(1));
    }

    [Test]
    public async Task Grant_actually_credits_rupy_for_day1()
    {
        var (factory, viewerId) = await SetupAsync();
        using var _ = factory;

        long before;
        using (var s = factory.Services.CreateScope())
        {
            var db = s.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var v = await db.Viewers.Include(x => x.Currency).FirstAsync(v => v.Id == viewerId);
            before = (long)v.Currency.Rupees;
        }

        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var bonus = scope.ServiceProvider.GetRequiredService<ILoginBonusService>();
        await using var tx = await inv.BeginAsync(viewerId);
        await bonus.GrantIfDueAsync(tx);
        await tx.CommitAsync();

        using var s2 = factory.Services.CreateScope();
        var db2 = s2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var after = (long)(await db2.Viewers.Include(x => x.Currency).FirstAsync(v => v.Id == viewerId)).Currency.Rupees;

        Assert.That(after - before, Is.EqualTo(20), "Day 1 = 20 Rupy per catalog");
    }

    [Test]
    public async Task IsDue_is_false_after_claim_same_day()
    {
        var (factory, viewerId) = await SetupAsync();
        using var _ = factory;

        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var bonus = scope.ServiceProvider.GetRequiredService<ILoginBonusService>();

        var v0 = await ReloadViewerAsync(factory, viewerId);
        Assert.That(bonus.IsDue(v0), Is.True, "fresh viewer is due");

        await using var tx = await inv.BeginAsync(viewerId);
        await bonus.GrantIfDueAsync(tx);
        await tx.CommitAsync();

        var v1 = await ReloadViewerAsync(factory, viewerId);
        Assert.That(bonus.IsDue(v1), Is.False);
    }
}
