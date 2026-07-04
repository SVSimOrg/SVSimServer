using SVSim.Database.Services.Replay;

namespace SVSim.UnitTests.Services;

public class InMemoryBattleContextStoreTests
{
    private static BattleContext MakeCtx(long battleId = 1, int oppViewer = 0) => new(
        BattleId: battleId,
        BattleType: 2,
        DeckFormat: 0,
        TwoPickType: 0,
        SelfClassId: 1,
        SelfSubClassId: 0,
        SelfCharaId: 1,
        SelfRotationId: "0",
        OpponentViewerId: oppViewer,
        OpponentName: "Opponent",
        OpponentClassId: 2,
        OpponentSubClassId: 0,
        OpponentCharaId: 1,
        OpponentCountryCode: "",
        OpponentEmblemId: 0,
        OpponentDegreeId: 0,
        OpponentRotationId: "0",
        BattleStartTime: DateTime.UtcNow);

    [Test]
    public void Set_then_TakeFor_returns_ctx_and_clears()
    {
        var store = new InMemoryBattleContextStore();
        var ctx = MakeCtx(battleId: 42);

        store.Set(viewerId: 100, ctx);
        var taken = store.TakeFor(100);

        Assert.That(taken, Is.Not.Null);
        Assert.That(taken!.BattleId, Is.EqualTo(42));
        Assert.That(store.TakeFor(100), Is.Null, "second take must return null");
    }

    [Test]
    public void TakeFor_missing_viewer_returns_null()
    {
        var store = new InMemoryBattleContextStore();
        Assert.That(store.TakeFor(999), Is.Null);
    }

    [Test]
    public void Set_overwrites_prior_context_for_same_viewer()
    {
        var store = new InMemoryBattleContextStore();
        store.Set(viewerId: 100, MakeCtx(battleId: 1));
        store.Set(viewerId: 100, MakeCtx(battleId: 2));

        var taken = store.TakeFor(100);
        Assert.That(taken!.BattleId, Is.EqualTo(2));
    }
}
