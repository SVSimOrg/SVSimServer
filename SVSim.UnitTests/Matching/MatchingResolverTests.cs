using Moq;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.EmulatedEntrypoint.Matching;

namespace SVSim.UnitTests.Matching;

/// <summary>
/// Per-test locals (no fixture-level fields) because the assembly runs with
/// <c>[Parallelizable(ParallelScope.All)]</c> — shared <c>_resolver</c>/<c>_bridge</c>
/// fields would race across concurrent tests in this fixture.
/// </summary>
[TestFixture]
public class MatchingResolverTests
{
    private sealed record Harness(
        Mock<IMatchingBridge> Bridge,
        Mock<IMatchingPairUpService> PairUp,
        MatchingResolver Resolver);

    private static Harness BuildHarness()
    {
        var bridge = new Mock<IMatchingBridge>(MockBehavior.Strict);
        var pairUp = new Mock<IMatchingPairUpService>(MockBehavior.Strict);
        return new Harness(bridge, pairUp, new MatchingResolver(bridge.Object, pairUp.Object));
    }

    private static BattlePlayer Player(long vid = 1) =>
        new(vid, new MatchContext(
            SelfDeckCardIds: Array.Empty<long>(), ClassId: CardClass.None, CharaId: "0",
            CardMasterName: "card_master_node_10015",
            CountryCode: "JP", UserName: $"P{vid}", SleeveId: "0",
            EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo));

    [Test]
    public async Task When_neither_flag_set_calls_pairUp_and_parks_returns_3002_with_empty_url()
    {
        var h = BuildHarness();
        var player = Player();
        h.PairUp.Setup(p => p.TryPairAsync("arena_two_pick_battle", player, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PairUpResult?)null);

        var r = await h.Resolver.ResolveAsync("arena_two_pick_battle", player, default);

        Assert.That(r.MatchingState, Is.EqualTo(3002));
        Assert.That(r.BattleId, Is.Null);
        Assert.That(r.NodeServerUrl, Is.EqualTo(""), "Empty string (not null) — client unguarded-.ToString()s it.");
        h.Bridge.Verify(b => b.RegisterBattle(It.IsAny<BattlePlayer>(), It.IsAny<BattlePlayer?>(), It.IsAny<BattleType>()), Times.Never);
    }

    [Test]
    public async Task Pair_owner_role_returns_3007()
    {
        var h = BuildHarness();
        var player = Player();
        h.PairUp.Setup(p => p.TryPairAsync("rotation_rank_battle", player, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PairUpResult(new PendingMatch("bid-x", "node.local/socket.io/"), IsOwner: true, IsAiFallback: false));

        var r = await h.Resolver.ResolveAsync("rotation_rank_battle", player, default);

        Assert.That(r.MatchingState, Is.EqualTo(3007));
        Assert.That(r.BattleId, Is.EqualTo("bid-x"));
    }

    [Test]
    public async Task Pair_joiner_role_returns_3004()
    {
        var h = BuildHarness();
        var player = Player();
        h.PairUp.Setup(p => p.TryPairAsync("rotation_rank_battle", player, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PairUpResult(new PendingMatch("bid-x", "node.local/socket.io/"), IsOwner: false, IsAiFallback: false));

        var r = await h.Resolver.ResolveAsync("rotation_rank_battle", player, default);

        Assert.That(r.MatchingState, Is.EqualTo(3004));
    }

    [Test]
    public async Task AI_fallback_returns_3011_regardless_of_owner_flag()
    {
        // IsAiFallback wins the switch even if IsOwner is also true (the resolver's first arm).
        var h = BuildHarness();
        var player = Player();
        h.PairUp.Setup(p => p.TryPairAsync("unlimited_rank_battle", player, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PairUpResult(new PendingMatch("bid-ai", "node.local/socket.io/"), IsOwner: true, IsAiFallback: true));

        var r = await h.Resolver.ResolveAsync("unlimited_rank_battle", player, default);

        Assert.That(r.MatchingState, Is.EqualTo(3011));
    }
}
