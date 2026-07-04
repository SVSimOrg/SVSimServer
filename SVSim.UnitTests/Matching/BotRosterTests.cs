using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.Database.Repositories.Globals;
using SVSim.EmulatedEntrypoint.Matching;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Matching;

[TestFixture]
public class BotRosterTests
{
    private static MatchContext Ctx(string userName, CardClass classId) => new(
        SelfDeckCardIds: Array.Empty<long>(),
        ClassId: classId, CharaId: classId.ToWireValue(), CardMasterName: "card_master_node_10015",
        CountryCode: "JP", UserName: userName, SleeveId: "0",
        EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo);

    private static async Task<BotRoster> NewRosterAsync(SVSimTestFactory factory)
    {
        await factory.SeedGlobalsAsync();
        var scope = factory.Services.CreateScope();
        var globals = scope.ServiceProvider.GetRequiredService<IGlobalsRepository>();
        return new BotRoster(globals);
    }

    [Test]
    public async Task PickAsync_returns_a_bot_with_valid_ai_id_from_rm_ai_setting()
    {
        using var factory = new SVSimTestFactory();
        var roster = await NewRosterAsync(factory);

        var bot = await roster.PickAsync(Ctx("PlayerA", CardClass.Forestcraft), "123456789012");

        // Series-1 enemy_ai_id values from data_dumps/client-assets/rm_ai_setting.csv —
        // one per class (1=Forest, 2=Sword, 3=Rune, 4=Dragon, 5=Shadow, 6=Blood, 7=Haven, 8=Portal).
        // Must match a real row or the client's RankMatchAISettingList.GetSettingData() throws.
        Assert.That(bot.AiId, Is.AnyOf(1111, 1121, 1131, 1141, 1151, 1161, 1171, 1181));
    }

    [Test]
    public async Task PickAsync_returns_bot_with_class_metadata_set()
    {
        using var factory = new SVSimTestFactory();
        var roster = await NewRosterAsync(factory);

        var bot = await roster.PickAsync(Ctx("PlayerA", CardClass.Forestcraft), "123456789012");

        Assert.That(bot.ClassId, Is.InRange(1, 8));
        Assert.That(bot.CharaId, Is.InRange(1, 8));
        Assert.That(bot.UserName, Is.Not.Null.And.Not.Empty);
        Assert.That(bot.CountryCode, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task PickAsync_is_deterministic_per_battle_id()
    {
        using var factory = new SVSimTestFactory();
        var roster = await NewRosterAsync(factory);
        var ctx = Ctx("PlayerA", CardClass.Runecraft);

        var a = await roster.PickAsync(ctx, "999888777666");
        var b = await roster.PickAsync(ctx, "999888777666");

        Assert.That(a, Is.EqualTo(b), "Same battleId → same bot, so mid-flight retries get the same opponent.");
    }

    [Test]
    public async Task PickAsync_varies_across_different_battle_ids()
    {
        using var factory = new SVSimTestFactory();
        var roster = await NewRosterAsync(factory);
        var ctx = Ctx("PlayerA", CardClass.Runecraft);

        var seen = new HashSet<int>();
        for (var i = 0; i < 20; i++)
        {
            var bot = await roster.PickAsync(ctx, $"{100000000000 + i}");
            seen.Add(bot.AiId);
        }

        Assert.That(seen.Count, Is.GreaterThan(1), "Different battle IDs should pick different bots.");
    }

    [Test]
    public async Task PickAsync_throws_when_roster_empty()
    {
        // Empty DB (no SeedGlobalsAsync call) → no rows → invariant violated.
        using var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var globals = scope.ServiceProvider.GetRequiredService<IGlobalsRepository>();
        var roster = new BotRoster(globals);

        Assert.That(async () => await roster.PickAsync(Ctx("PlayerA", CardClass.Forestcraft), "000000000001"),
            Throws.InvalidOperationException);
    }
}
