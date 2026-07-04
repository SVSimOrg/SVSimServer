using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;

namespace SVSim.UnitTests.BattleNode.Sessions;

[TestFixture]
public class InMemoryBattleSessionStoreTests
{
    [Test]
    public void TryRegisterThenGet_ReturnsRegisteredBattle()
    {
        var store = new InMemoryBattleSessionStore();
        var battle = new PendingBattle("bid-1", BattleType.Bot, new BattlePlayer(906243102, FixtureCtx()), null);
        Assert.That(store.TryRegisterPending(battle), Is.True);

        Assert.That(store.TryGetPending("bid-1"), Is.EqualTo(battle));
    }

    [Test]
    public void Get_UnknownBattleId_ReturnsNull()
    {
        var store = new InMemoryBattleSessionStore();
        Assert.That(store.TryGetPending("nope"), Is.Null);
    }

    [Test]
    public void Remove_ReturnsTrueWhenPresent_FalseWhenAbsent()
    {
        var store = new InMemoryBattleSessionStore();
        store.TryRegisterPending(new PendingBattle("bid", BattleType.Bot, new BattlePlayer(1, FixtureCtx()), null));
        Assert.That(store.RemovePending("bid"), Is.True);
        Assert.That(store.RemovePending("bid"), Is.False);
    }

    [Test]
    public void TryRegister_DuplicateBattleId_ReturnsFalseAndPreservesOriginal()
    {
        var store = new InMemoryBattleSessionStore();
        store.TryRegisterPending(new PendingBattle("bid", BattleType.Bot, new BattlePlayer(1, FixtureCtx()), null));
        var second = store.TryRegisterPending(new PendingBattle("bid", BattleType.Bot, new BattlePlayer(2, FixtureCtx()), null));
        Assert.That(second, Is.False);
        Assert.That(store.TryGetPending("bid")!.P1.ViewerId, Is.EqualTo(1));
    }

    private static MatchContext FixtureCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(i => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);
}
