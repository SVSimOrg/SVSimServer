using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Persistence;

public class SerialCodePersistenceTests
{
    [Test]
    public async Task SerialCode_round_trips_with_rewards()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            ctx.SerialCodes.Add(new SerialCodeEntry
            {
                Code = "ABCD-1234",
                Message = "Test code",
                IsEnabled = true,
                Rewards =
                {
                    new SerialCodeRewardEntry { Slot = 0, RewardType = 1, RewardDetailId = 0, RewardCount = 100 },
                    new SerialCodeRewardEntry { Slot = 1, RewardType = 9, RewardDetailId = 0, RewardCount = 500 },
                },
            });
            await ctx.SaveChangesAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var ctx2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var code = await ctx2.SerialCodes
            .Include(c => c.Rewards.OrderBy(r => r.Slot))
            .AsNoTracking()
            .FirstAsync(c => c.Code == "ABCD-1234");

        Assert.That(code.Message, Is.EqualTo("Test code"));
        Assert.That(code.IsEnabled, Is.True);
        Assert.That(code.Rewards, Has.Count.EqualTo(2));
        Assert.That(code.Rewards[0].RewardCount, Is.EqualTo(100));
        Assert.That(code.Rewards[1].RewardCount, Is.EqualTo(500));
    }

    [Test]
    public async Task Unique_constraint_on_Code_rejects_duplicates()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        ctx.SerialCodes.Add(new SerialCodeEntry { Code = "DUP", Message = "first", IsEnabled = true });
        await ctx.SaveChangesAsync();

        ctx.SerialCodes.Add(new SerialCodeEntry { Code = "DUP", Message = "second", IsEnabled = true });
        Assert.That(async () => await ctx.SaveChangesAsync(), Throws.Exception);
    }

    [Test]
    public async Task Composite_PK_on_redemption_rejects_double_redeem()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        int codeId;
        using (var seedScope = factory.Services.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var code = new SerialCodeEntry { Code = "ONCE", Message = "single use", IsEnabled = true };
            ctx.SerialCodes.Add(code);
            await ctx.SaveChangesAsync();
            codeId = code.Id;

            ctx.ViewerSerialCodeRedemptions.Add(new ViewerSerialCodeRedemption
            {
                ViewerId = viewerId, SerialCodeId = codeId, RedeemedAt = DateTime.UtcNow,
            });
            await ctx.SaveChangesAsync();
        }

        // Second redemption attempt in a fresh scope so the change tracker doesn't intercept
        // before the DB constraint fires.
        using var dupeScope = factory.Services.CreateScope();
        var ctx2 = dupeScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        ctx2.ViewerSerialCodeRedemptions.Add(new ViewerSerialCodeRedemption
        {
            ViewerId = viewerId, SerialCodeId = codeId, RedeemedAt = DateTime.UtcNow,
        });
        Assert.That(async () => await ctx2.SaveChangesAsync(), Throws.Exception);
    }
}
