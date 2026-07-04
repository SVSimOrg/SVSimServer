using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Dispatch;

namespace SVSim.UnitTests.BattleNode.Sessions;

[TestFixture]
public class BattleSessionStateTests
{
    private sealed class StubParticipant : IBattleParticipant
    {
        public long ViewerId { get; }
        public MatchContext Context { get; }
        public event Func<SVSim.BattleNode.Protocol.MsgEnvelope, CancellationToken, Task>? FrameEmitted;
        public StubParticipant(long id, MatchContext ctx) { ViewerId = id; Context = ctx; }
        public Task PushAsync(SVSim.BattleNode.Protocol.MsgEnvelope e, Stock n, CancellationToken c) => Task.CompletedTask;
        public Task RunAsync(CancellationToken c) => Task.CompletedTask;
        public Task TerminateAsync(BattleFinishReason r) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        private void Touch() => FrameEmitted?.Invoke(null!, default);
    }

    private static MatchContext Ctx(params long[] deck) => new(
        SelfDeckCardIds: deck, ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "cm",
        CountryCode: CountryCodes.Korea, UserName: "P", SleeveId: "0", EmblemId: "0", DegreeId: "0",
        FieldId: 0, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo);

    [Test]
    public void GetOrSeedDeckMap_maps_idx_1based_to_the_shuffled_order()
    {
        // The map seeds from GetShuffledDeck, not raw build order. idx (i+1) -> shuffledDeck[i],
        // and the set of cardIds is unchanged (1..3 present, 4 absent).
        var state = new BattleSessionState(masterSeed: 12345);
        var p = new StubParticipant(1, Ctx(900L, 901L, 902L));
        var shuffled = state.GetShuffledDeck(p);

        var map = state.GetOrSeedDeckMap(p);

        Assert.That(map[1], Is.EqualTo(shuffled[0]));
        Assert.That(map[2], Is.EqualTo(shuffled[1]));
        Assert.That(map[3], Is.EqualTo(shuffled[2]));
        Assert.That(map.ContainsKey(4), Is.False);
        Assert.That(new[] { map[1], map[2], map[3] }, Is.EquivalentTo(new[] { 900L, 901L, 902L }));
    }

    [Test]
    public void GetOrSeedDeckMap_is_idempotent_same_instance()
    {
        var state = new BattleSessionState();
        var p = new StubParticipant(1, Ctx(900L));
        Assert.That(state.GetOrSeedDeckMap(p), Is.SameAs(state.GetOrSeedDeckMap(p)));
    }

    [Test]
    public void GetShuffledDeck_is_a_permutation_of_the_input()
    {
        var state = new BattleSessionState(masterSeed: 12345);
        var p = new StubParticipant(1001, Ctx(DistinctDeck()));

        Assert.That(state.GetShuffledDeck(p), Is.EquivalentTo(DistinctDeck()),
            "same multiset of cards, just reordered");
    }

    [Test]
    public void GetShuffledDeck_actually_reorders_a_distinct_deck()
    {
        var state = new BattleSessionState(masterSeed: 12345);
        var p = new StubParticipant(1001, Ctx(DistinctDeck()));

        Assert.That(state.GetShuffledDeck(p), Is.Not.EqualTo(DistinctDeck()),
            "a 30-card distinct deck should not survive the shuffle in original order");
    }

    [Test]
    public void GetShuffledDeck_is_deterministic_for_same_master_seed_and_viewer()
    {
        var a = new BattleSessionState(masterSeed: 777).GetShuffledDeck(new StubParticipant(1001, Ctx(DistinctDeck())));
        var b = new BattleSessionState(masterSeed: 777).GetShuffledDeck(new StubParticipant(1001, Ctx(DistinctDeck())));
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void GetShuffledDeck_differs_across_master_seeds()
    {
        var a = new BattleSessionState(masterSeed: 1).GetShuffledDeck(new StubParticipant(1001, Ctx(DistinctDeck())));
        var b = new BattleSessionState(masterSeed: 2).GetShuffledDeck(new StubParticipant(1001, Ctx(DistinctDeck())));
        Assert.That(a, Is.Not.EqualTo(b));
    }

    private static long[] DistinctDeck() =>
        Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToArray();
}
