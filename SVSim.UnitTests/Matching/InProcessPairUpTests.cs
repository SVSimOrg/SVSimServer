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
public class InProcessPairUpTests
{
    [Test]
    public async Task TryPairAsync_on_empty_slot_returns_null_and_parks()
    {
        var svc = BuildSvc();

        var match = await svc.TryPairAsync("tk2", new BattlePlayer(1, Ctx()), CancellationToken.None);

        Assert.That(match, Is.Null);
    }

    [Test]
    public async Task TryPairAsync_with_waiting_partner_pairs_returns_match_as_joiner()
    {
        var svc = BuildSvc();

        await svc.TryPairAsync("tk2", new BattlePlayer(1, Ctx()), CancellationToken.None);
        var result = await svc.TryPairAsync("tk2", new BattlePlayer(2, Ctx()), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Match.BattleId, Is.Not.Empty);
        Assert.That(result.IsOwner, Is.False,
            "The second arriver (who triggered the pair) is the joiner — wire matching_state 3004.");
        Assert.That(result.IsAiFallback, Is.False,
            "TK2 is PvpOnly — never falls back to AI.");
    }

    [Test]
    public async Task First_arrivers_next_poll_returns_cached_match_as_owner_then_evicts()
    {
        var svc = BuildSvc();

        await svc.TryPairAsync("tk2", new BattlePlayer(1, Ctx()), CancellationToken.None);  // park
        var secondPaired = await svc.TryPairAsync("tk2", new BattlePlayer(2, Ctx()), CancellationToken.None);  // pair
        var firstCached = await svc.TryPairAsync("tk2", new BattlePlayer(1, Ctx()), CancellationToken.None);   // consume
        var firstAgain = await svc.TryPairAsync("tk2", new BattlePlayer(1, Ctx()), CancellationToken.None);    // post-consume

        Assert.That(firstCached, Is.Not.Null);
        Assert.That(firstCached!.Match.BattleId, Is.EqualTo(secondPaired!.Match.BattleId));
        Assert.That(firstCached.IsOwner, Is.True,
            "The first arriver picking up their cached pair is the owner — wire matching_state 3007.");
        Assert.That(secondPaired.IsOwner, Is.False,
            "Sanity: the same pair-up returns IsOwner=true to the cached/first arriver and IsOwner=false to the joiner.");
        Assert.That(firstAgain, Is.Null, "Consumed entry must be evicted; next call re-parks.");
    }

    [Test]
    public async Task Different_modes_do_not_pair_across_slots()
    {
        var svc = BuildSvc();

        await svc.TryPairAsync("tk2", new BattlePlayer(1, Ctx()), CancellationToken.None);
        var rankMatch = await svc.TryPairAsync("rank_rotation", new BattlePlayer(2, Ctx()), CancellationToken.None);

        Assert.That(rankMatch, Is.Null, "Different mode shouldn't pair with tk2's waiting viewer.");
    }

    /// <summary>
    /// Builds an InProcessPairUp with a real MatchingBridge (so BattleIds are real)
    /// + a fake clock, default-threshold MatchingConfig, and an empty policy registry
    /// (so unknown modes default to PvpOnly — preserving Phase 2 behaviour for
    /// these legacy tests).
    /// </summary>
    private static InProcessPairUp BuildSvc()
    {
        var bridge = new MatchingBridge(new InMemoryBattleSessionStore(), new BattleNodeOptions());
        var clock = new FakeTimeProvider();
        var config = new Mock<IGameConfigService>();
        config.Setup(c => c.Get<MatchingConfig>()).Returns(new MatchingConfig());
        var policies = new ModePolicyRegistry(Array.Empty<ModePolicy>());

        var services = new ServiceCollection();
        services.AddScoped<IGameConfigService>(_ => config.Object);
        var sp = services.BuildServiceProvider();
        return new InProcessPairUp(bridge, policies, sp.GetRequiredService<IServiceScopeFactory>(), clock);
    }

    private static MatchContext Ctx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);
}
