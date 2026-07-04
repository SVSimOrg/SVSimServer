using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;

namespace SVSim.UnitTests.BattleNode.Bridge;

[TestFixture]
public class MatchingBridgeTests
{
    [Test]
    public void RegisterBattle_Bot_stores_pending_and_returns_node_url()
    {
        var store = new InMemoryBattleSessionStore();
        var bridge = new MatchingBridge(store, new BattleNodeOptions { NodeServerUrl = "localhost:5148/socket.io/" });
        var p1 = new BattlePlayer(906243102, FixtureCtx());

        var match = bridge.RegisterBattle(p1, p2: null, BattleType.Bot);

        Assert.That(match.NodeServerUrl, Is.EqualTo("localhost:5148/socket.io/"));
        Assert.That(match.BattleId, Is.Not.Empty);
        var pending = store.TryGetPending(match.BattleId);
        Assert.That(pending, Is.Not.Null);
        Assert.That(pending!.Type, Is.EqualTo(BattleType.Bot));
        Assert.That(pending.P1.ViewerId, Is.EqualTo(906243102));
        Assert.That(pending.P2, Is.Null);
    }

    [Test]
    public void RegisterBattle_mints_unique_BattleIds_per_call()
    {
        var bridge = new MatchingBridge(new InMemoryBattleSessionStore(), new BattleNodeOptions());

        var a = bridge.RegisterBattle(new BattlePlayer(1, FixtureCtx()), null, BattleType.Bot);
        var b = bridge.RegisterBattle(new BattlePlayer(2, FixtureCtx()), null, BattleType.Bot);

        Assert.That(a.BattleId, Is.Not.EqualTo(b.BattleId));
    }

    [Test]
    public void RegisterBattle_produces_twelve_digit_decimal_BattleId()
    {
        var bridge = new MatchingBridge(new InMemoryBattleSessionStore(), new BattleNodeOptions());

        var match = bridge.RegisterBattle(new BattlePlayer(1, FixtureCtx()), null, BattleType.Bot);

        Assert.That(match.BattleId, Has.Length.EqualTo(12));
        Assert.That(match.BattleId, Does.Match("^[0-9]{12}$"));
    }

    [Test]
    public void RegisterBattle_Pvp_requires_both_players()
    {
        var bridge = new MatchingBridge(new InMemoryBattleSessionStore(), new BattleNodeOptions());

        Assert.Throws<ArgumentException>(() =>
            bridge.RegisterBattle(new BattlePlayer(1, FixtureCtx()), p2: null, BattleType.Pvp));
    }

    [Test]
    public void RegisterBattle_Pvp_rejects_same_viewer_twice()
    {
        var bridge = new MatchingBridge(new InMemoryBattleSessionStore(), new BattleNodeOptions());

        Assert.Throws<ArgumentException>(() =>
            bridge.RegisterBattle(
                new BattlePlayer(1, FixtureCtx()), new BattlePlayer(1, FixtureCtx()), BattleType.Pvp));
    }

    [Test]
    public void RegisterBattle_Bot_rejects_non_null_p2()
    {
        var bridge = new MatchingBridge(new InMemoryBattleSessionStore(), new BattleNodeOptions());

        Assert.Throws<ArgumentException>(() =>
            bridge.RegisterBattle(
                new BattlePlayer(1, FixtureCtx()), new BattlePlayer(2, FixtureCtx()), BattleType.Bot));
    }

    [Test]
    public void RegisterBattle_evicts_stale_pending_for_same_viewer()
    {
        var store = new InMemoryBattleSessionStore();
        var bridge = new MatchingBridge(store, new BattleNodeOptions());
        var p1 = new BattlePlayer(42, FixtureCtx());

        var first = bridge.RegisterBattle(p1, p2: null, BattleType.Bot);
        Assert.That(store.TryGetPending(first.BattleId), Is.Not.Null);

        var second = bridge.RegisterBattle(p1, p2: null, BattleType.Bot);
        Assert.That(store.TryGetPending(first.BattleId), Is.Null, "stale entry must be evicted");
        Assert.That(store.TryGetPending(second.BattleId), Is.Not.Null);
    }

    private static MatchContext FixtureCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(i => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);
}
