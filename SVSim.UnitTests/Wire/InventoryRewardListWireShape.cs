using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Wire;

public class InventoryRewardListWireShape
{
    [Test]
    public async Task Spend_crystal_plus_grant_card_with_cascade_matches_fixture()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        long cardId = await factory.SeedCardAsync();
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var v = await ctx.Viewers.FirstAsync(x => x.Id == viewerId);
        v.Currency.Crystals = 1000;
        const int sleeveId = 2_000_040_000;
        ctx.Sleeves.Add(new SleeveEntry { Id = sleeveId });
        ctx.CardCosmeticRewards.Add(new CardCosmeticReward { CardId = cardId, CosmeticId = sleeveId, Type = CosmeticType.Sleeve });
        await ctx.SaveChangesAsync();

        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        await using var tx = await inv.BeginAsync(viewerId);
        await tx.TrySpendAsync(SpendCurrency.Crystal, 500);
        await tx.GrantAsync(UserGoodsType.Card, cardId, 3);
        var result = await tx.CommitAsync();

        var opts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        var json = JsonSerializer.Serialize(result.RewardList, opts);

        // Expected order: currency entries in first-touch order, then non-currency in first-touch order.
        // Crystal spend comes first (post-state 500), then Card grant (post-state count 3), then
        // Sleeve cascade (always 1).
        var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.EnumerateArray().ToList();
        Assert.That(arr, Has.Count.EqualTo(3), $"Expected 3 reward entries, got {arr.Count}. JSON: {json}");

        Assert.That(arr[0].GetProperty("reward_type").GetInt32(), Is.EqualTo((int)UserGoodsType.Crystal),
            "First entry should be Crystal (spend post-state)");
        Assert.That(arr[0].GetProperty("reward_num").GetInt32(), Is.EqualTo(500),
            "Crystal post-state after spending 500 from 1000 should be 500");

        Assert.That(arr[1].GetProperty("reward_type").GetInt32(), Is.EqualTo((int)UserGoodsType.Card),
            "Second entry should be Card");
        Assert.That(arr[1].GetProperty("reward_num").GetInt32(), Is.EqualTo(3),
            "Card post-state count for fresh grant of 3 should be 3");

        Assert.That(arr[2].GetProperty("reward_type").GetInt32(), Is.EqualTo((int)UserGoodsType.Sleeve),
            "Third entry should be Sleeve (cascade from card grant)");
        Assert.That(arr[2].GetProperty("reward_id").GetInt32(), Is.EqualTo(sleeveId),
            "Sleeve reward_id should match the seeded sleeve");

        // Verify snake_case keys are present (not PascalCase)
        Assert.That(arr[0].TryGetProperty("reward_type", out _), Is.True, "Key must be reward_type not RewardType");
        Assert.That(arr[0].TryGetProperty("reward_id", out _), Is.True, "Key must be reward_id not RewardId");
        Assert.That(arr[0].TryGetProperty("reward_num", out _), Is.True, "Key must be reward_num not RewardNum");
    }
}
