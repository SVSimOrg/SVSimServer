using NUnit.Framework;
using SVSim.EmulatedEntrypoint.Matching;

namespace SVSim.UnitTests.Matching;

[TestFixture]
public class ModePolicyRegistryTests
{
    [Test]
    public void For_known_mode_returns_its_policy()
    {
        var reg = new ModePolicyRegistry(new[]
        {
            new ModePolicy("rotation_rank_battle", PolicyKind.PvpFirstThenAiFallback),
            new ModePolicy("arena_two_pick_battle", PolicyKind.PvpOnly),
        });

        Assert.That(reg.For("rotation_rank_battle").Kind, Is.EqualTo(PolicyKind.PvpFirstThenAiFallback));
        Assert.That(reg.For("arena_two_pick_battle").Kind, Is.EqualTo(PolicyKind.PvpOnly));
    }

    [Test]
    public void For_unknown_mode_returns_PvpOnly_default()
    {
        var reg = new ModePolicyRegistry(Array.Empty<ModePolicy>());

        var policy = reg.For("free_battle");

        Assert.That(policy.Mode, Is.EqualTo("free_battle"));
        Assert.That(policy.Kind, Is.EqualTo(PolicyKind.PvpOnly));
    }

    [Test]
    public void Last_registration_for_same_mode_wins()
    {
        // Defensive: if someone double-registers a mode, the dict semantics
        // give us the last one. Document the behavior in a test.
        var reg = new ModePolicyRegistry(new[]
        {
            new ModePolicy("rotation_rank_battle", PolicyKind.PvpOnly),
            new ModePolicy("rotation_rank_battle", PolicyKind.PvpFirstThenAiFallback),
        });

        Assert.That(reg.For("rotation_rank_battle").Kind, Is.EqualTo(PolicyKind.PvpFirstThenAiFallback));
    }
}
