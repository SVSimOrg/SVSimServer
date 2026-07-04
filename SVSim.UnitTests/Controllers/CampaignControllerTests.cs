using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class CampaignControllerTests
{
    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<SerialCodeEntry> SeedCodeAsync(
        SVSimTestFactory factory, string code, string message = "test message",
        bool enabled = true, DateTime? startAt = null, DateTime? endAt = null,
        params (int Type, long DetailId, int Count)[] rewards)
    {
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var entity = new SerialCodeEntry
        {
            Code = code,
            Message = message,
            IsEnabled = enabled,
            StartAt = startAt,
            EndAt = endAt,
        };
        for (int i = 0; i < rewards.Length; i++)
        {
            entity.Rewards.Add(new SerialCodeRewardEntry
            {
                Slot = i,
                RewardType = rewards[i].Type,
                RewardDetailId = rewards[i].DetailId,
                RewardCount = rewards[i].Count,
            });
        }
        ctx.SerialCodes.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    [Test]
    public async Task Register_with_valid_unredeemed_code_returns_success_and_creates_presents()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        var code = await SeedCodeAsync(factory, "VALID1", "Welcome reward",
            rewards: new[] { (1, 0L, 100), (9, 0L, 500) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"VALID1"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("is_complete").GetBoolean(), Is.True);

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var presents = await ctx.ViewerPresents.AsNoTracking()
            .Where(p => p.ViewerId == viewerId).OrderBy(p => p.RewardType).ToListAsync();
        Assert.That(presents, Has.Count.EqualTo(2));
        Assert.That(presents.All(p => p.Message == "Welcome reward"), Is.True);
        Assert.That(presents.All(p => p.Source == $"serial_code:{code.Id}"), Is.True);
        Assert.That(presents.All(p => p.Status == PresentStatus.Unclaimed), Is.True);

        var redemptions = await ctx.ViewerSerialCodeRedemptions.AsNoTracking()
            .Where(r => r.ViewerId == viewerId && r.SerialCodeId == code.Id).ToListAsync();
        Assert.That(redemptions, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Register_with_unknown_code_returns_4202()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"NOSUCHCODE"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));
    }

    [Test]
    public async Task Register_with_disabled_code_returns_4202()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCodeAsync(factory, "DISABLED", enabled: false,
            rewards: new[] { (1, 0L, 100) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"DISABLED"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));
    }

    [Test]
    public async Task Register_with_pre_start_code_returns_4202()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCodeAsync(factory, "FUTURE",
            startAt: DateTime.UtcNow.AddDays(1),
            rewards: new[] { (1, 0L, 100) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"FUTURE"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));
    }

    [Test]
    public async Task Register_with_expired_code_returns_4202()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCodeAsync(factory, "EXPIRED",
            endAt: DateTime.UtcNow.AddDays(-1),
            rewards: new[] { (1, 0L, 100) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"EXPIRED"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));
    }

    [Test]
    public async Task Register_with_already_redeemed_code_returns_4202()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        var code = await SeedCodeAsync(factory, "ONCE",
            rewards: new[] { (1, 0L, 100) });

        // Pre-seed a redemption record.
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.ViewerSerialCodeRedemptions.Add(new ViewerSerialCodeRedemption
            {
                ViewerId = viewerId,
                SerialCodeId = code.Id,
                RedeemedAt = DateTime.UtcNow.AddMinutes(-5),
            });
            await ctx.SaveChangesAsync();
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"ONCE"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));

        // Sanity: no new presents created.
        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var presents = await ctx2.ViewerPresents.AsNoTracking()
            .Where(p => p.ViewerId == viewerId).CountAsync();
        Assert.That(presents, Is.EqualTo(0));
    }

    [Test]
    public async Task Register_with_unsupported_reward_type_returns_4202_and_no_redemption_row()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        // RewardType 11 = SpotCard; InventoryTransaction throws NotSupportedException, so the
        // gift mapper rejects it.
        var code = await SeedCodeAsync(factory, "BADTYPE",
            rewards: new[] { (11, 100L, 1) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"BADTYPE"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));

        // Critical: no redemption row created → admin can fix and player can retry.
        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var redemptions = await ctx.ViewerSerialCodeRedemptions.AsNoTracking()
            .Where(r => r.ViewerId == viewerId && r.SerialCodeId == code.Id).CountAsync();
        Assert.That(redemptions, Is.EqualTo(0));
    }

    [Test]
    public async Task Register_without_auth_returns_401()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateClient();  // no X-Test-Viewer-Id header

        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"WHATEVER"}"""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Register_is_case_sensitive_for_code_match()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        await SeedCodeAsync(factory, "MixedCase",
            rewards: new[] { (1, 0L, 100) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"mixedcase"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("result_code").GetInt32(), Is.EqualTo(4202));
    }

    [Test]
    public async Task Register_with_card_reward_succeeds_and_creates_present()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        var code = await SeedCodeAsync(factory, "CARDCODE", "Free card",
            rewards: new[] { (5, 12345L, 1) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"CARDCODE"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("is_complete").GetBoolean(), Is.True);

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var presents = await ctx.ViewerPresents.AsNoTracking()
            .Where(p => p.ViewerId == viewerId).ToListAsync();
        Assert.That(presents, Has.Count.EqualTo(1));
        Assert.That(presents[0].RewardType, Is.EqualTo(5));
        Assert.That(presents[0].RewardDetailId, Is.EqualTo(12345L));
        Assert.That(presents[0].Source, Is.EqualTo($"serial_code:{code.Id}"));
    }

    [Test]
    public async Task Register_with_sleeve_reward_succeeds_and_creates_present()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        var code = await SeedCodeAsync(factory, "SLEEVECODE", "Free sleeve",
            rewards: new[] { (6, 700100L, 1) });

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/campaign/regist_serial_code",
            JsonBody("""{"serial_code":"SLEEVECODE"}"""));
        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), raw);

        using var doc = JsonDocument.Parse(raw);
        Assert.That(doc.RootElement.GetProperty("is_complete").GetBoolean(), Is.True);

        using var verifyScope = factory.Services.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var presents = await ctx.ViewerPresents.AsNoTracking()
            .Where(p => p.ViewerId == viewerId).ToListAsync();
        Assert.That(presents, Has.Count.EqualTo(1));
        Assert.That(presents[0].RewardType, Is.EqualTo(6));
        Assert.That(presents[0].RewardDetailId, Is.EqualTo(700100L));
    }
}
