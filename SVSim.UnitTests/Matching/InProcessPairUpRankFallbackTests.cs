using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Matching;

namespace SVSim.UnitTests.Matching;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class InProcessPairUpRankFallbackTests
{
    private FakeTimeProvider _clock = null!;
    private Mock<IMatchingBridge> _bridge = null!;
    private Mock<IGameConfigService> _config = null!;
    private ModePolicyRegistry _policies = null!;
    private InProcessPairUp _pairUp = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new FakeTimeProvider(startDateTime: new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero));
        _bridge = new Mock<IMatchingBridge>();
        _config = new Mock<IGameConfigService>();
        _config.Setup(c => c.Get<MatchingConfig>())
            .Returns(new MatchingConfig { RankBattleAiFallbackThresholdSeconds = 15 });
        _policies = new ModePolicyRegistry(new[]
        {
            new ModePolicy("rotation_rank_battle", PolicyKind.PvpFirstThenAiFallback),
            new ModePolicy("unlimited_rank_battle", PolicyKind.PvpFirstThenAiFallback),
            new ModePolicy("arena_two_pick_battle", PolicyKind.PvpOnly),
        });

        // Build a tiny service provider exposing the mock IGameConfigService as scoped,
        // and inject IServiceScopeFactory into InProcessPairUp the same way prod does.
        var services = new ServiceCollection();
        services.AddScoped<IGameConfigService>(_ => _config.Object);
        var sp = services.BuildServiceProvider();
        _pairUp = new InProcessPairUp(_bridge.Object, _policies, sp.GetRequiredService<IServiceScopeFactory>(), _clock);
    }

    private static BattlePlayer Player(long id) =>
        new(id, new MatchContext(
            SelfDeckCardIds: Array.Empty<long>(), ClassId: CardClass.None, CharaId: "0",
            CardMasterName: "card_master_node_10015",
            CountryCode: "JP", UserName: $"P{id}", SleeveId: "0",
            EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo));

    [Test]
    public async Task TK2_policy_is_PvpOnly_no_fallback_regression()
    {
        var p = Player(1);
        var first = await _pairUp.TryPairAsync("arena_two_pick_battle", p, default);
        Assert.That(first, Is.Null, "First poll should park.");

        _clock.Advance(TimeSpan.FromSeconds(20)); // Past the rotation threshold.
        var second = await _pairUp.TryPairAsync("arena_two_pick_battle", p, default);

        Assert.That(second, Is.Null, "TK2 must not fall back to AI even past threshold.");
        _bridge.Verify(b => b.RegisterBattle(It.IsAny<BattlePlayer>(), It.IsAny<BattlePlayer?>(), BattleType.Bot), Times.Never);
    }

    [Test]
    public async Task Rotation_first_poll_parks_no_fallback()
    {
        var p = Player(1);
        var result = await _pairUp.TryPairAsync("rotation_rank_battle", p, default);
        Assert.That(result, Is.Null, "First poll should park even on fallback-eligible modes.");
        _bridge.Verify(b => b.RegisterBattle(It.IsAny<BattlePlayer>(), It.IsAny<BattlePlayer?>(), It.IsAny<BattleType>()), Times.Never);
    }

    [Test]
    public async Task Rotation_second_poll_under_threshold_stays_parked()
    {
        var p = Player(1);
        await _pairUp.TryPairAsync("rotation_rank_battle", p, default);
        _clock.Advance(TimeSpan.FromSeconds(5));

        var result = await _pairUp.TryPairAsync("rotation_rank_battle", p, default);

        Assert.That(result, Is.Null, "Sub-threshold polls should keep the viewer parked.");
        _bridge.Verify(b => b.RegisterBattle(It.IsAny<BattlePlayer>(), It.IsAny<BattlePlayer?>(), It.IsAny<BattleType>()), Times.Never);
    }

    [Test]
    public async Task Rotation_poll_past_threshold_falls_back_to_Bot()
    {
        var p = Player(1);
        var bid = "bot-bid-1";
        var url = "http://node.local/socket.io/";
        _bridge.Setup(b => b.RegisterBattle(p, null, BattleType.Bot))
            .Returns(new PendingMatch(bid, url));

        await _pairUp.TryPairAsync("rotation_rank_battle", p, default);
        _clock.Advance(TimeSpan.FromSeconds(16));

        var result = await _pairUp.TryPairAsync("rotation_rank_battle", p, default);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsAiFallback, Is.True);
        Assert.That(result.IsOwner, Is.True);
        Assert.That(result.Match.BattleId, Is.EqualTo(bid));
        _bridge.Verify(b => b.RegisterBattle(p, null, BattleType.Bot), Times.Once);
    }

    [Test]
    public async Task Rotation_partner_arrives_before_threshold_pairs_PvP()
    {
        var pA = Player(1);
        var pB = Player(2);
        _bridge.Setup(b => b.RegisterBattle(pA, pB, BattleType.Pvp))
            .Returns(new PendingMatch("pvp-bid", "http://node.local/socket.io/"));

        await _pairUp.TryPairAsync("rotation_rank_battle", pA, default);
        _clock.Advance(TimeSpan.FromSeconds(10)); // Sub-threshold.
        var joinerResult = await _pairUp.TryPairAsync("rotation_rank_battle", pB, default);

        Assert.That(joinerResult, Is.Not.Null);
        Assert.That(joinerResult!.IsAiFallback, Is.False, "Pair-up wins over AI fallback when partner arrives in window.");
        Assert.That(joinerResult.IsOwner, Is.False, "Joiner role.");
        _bridge.Verify(b => b.RegisterBattle(pA, pB, BattleType.Pvp), Times.Once);
        _bridge.Verify(b => b.RegisterBattle(It.IsAny<BattlePlayer>(), null, BattleType.Bot), Times.Never);
    }

    [Test]
    public async Task Rotation_stale_waiter_evicted_on_next_arriver()
    {
        var pA = Player(1);
        var pB = Player(2);
        _bridge.Setup(b => b.RegisterBattle(It.IsAny<BattlePlayer>(), null, BattleType.Bot))
            .Returns<BattlePlayer, BattlePlayer?, BattleType>((p, _, _) => new PendingMatch("bot-" + p.ViewerId, "http://node.local/socket.io/"));

        await _pairUp.TryPairAsync("rotation_rank_battle", pA, default);
        _clock.Advance(TimeSpan.FromMinutes(6)); // Past the 5-minute stale eviction.

        var resultB = await _pairUp.TryPairAsync("rotation_rank_battle", pB, default);

        // B sees an empty slot (A evicted as stale) and becomes the new waiter.
        Assert.That(resultB, Is.Null);
        _bridge.Verify(b => b.RegisterBattle(pA, pB, BattleType.Pvp), Times.Never, "Stale A should not have paired with B.");
    }

    [Test]
    public async Task Unlimited_independent_from_Rotation()
    {
        var p = Player(1);
        await _pairUp.TryPairAsync("rotation_rank_battle", p, default);
        var unlimitedResult = await _pairUp.TryPairAsync("unlimited_rank_battle", p, default);

        Assert.That(unlimitedResult, Is.Null, "Per-mode slots must be independent.");
    }
}
