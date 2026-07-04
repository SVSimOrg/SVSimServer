using NUnit.Framework;
using SVSim.BattleNode.Lifecycle;

namespace SVSim.UnitTests.BattleNode.Lifecycle;

[TestFixture]
public class BattleSeedsTests
{
    // Golden values pin cross-run/cross-platform stability. They were computed from the exact
    // splitmix64 mix specified in BattleSeeds. If these ever change, replay reproducibility broke —
    // do NOT "update them to match"; find what changed the algorithm (e.g. someone slipped in
    // GetHashCode, which is per-process randomized).
    [Test]
    public void Derive_golden_values_are_stable()
    {
        Assert.That(BattleSeeds.Stable(12345), Is.EqualTo(1577307848));
        Assert.That(BattleSeeds.IdxChange(12345, 906243102), Is.EqualTo(1638231407));
        Assert.That(BattleSeeds.DeckShuffle(12345, 906243102), Is.EqualTo(355953180));
        Assert.That(BattleSeeds.IdxChange(12345, 847666884), Is.EqualTo(518125159));
        Assert.That(BattleSeeds.Stable(99999), Is.EqualTo(323349150));
    }

    [Test]
    public void Derive_is_deterministic_for_same_inputs()
    {
        Assert.That(BattleSeeds.Derive(7, "x", 42), Is.EqualTo(BattleSeeds.Derive(7, "x", 42)));
    }

    [Test]
    public void Derive_differs_across_tag_master_and_discriminator()
    {
        var baseline = BattleSeeds.Derive(7, "x", 42);
        Assert.That(BattleSeeds.Derive(8, "x", 42), Is.Not.EqualTo(baseline), "different master");
        Assert.That(BattleSeeds.Derive(7, "y", 42), Is.Not.EqualTo(baseline), "different tag");
        Assert.That(BattleSeeds.Derive(7, "x", 43), Is.Not.EqualTo(baseline), "different disc");
    }

    [Test]
    public void Derive_is_always_non_negative()
    {
        // System.Random tolerates any int, but a non-negative seed keeps parity with prod's
        // positive seed values and avoids surprises.
        Assert.That(BattleSeeds.Stable(int.MinValue), Is.GreaterThanOrEqualTo(0));
        Assert.That(BattleSeeds.Stable(-1), Is.GreaterThanOrEqualTo(0));
    }
}
