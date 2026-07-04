using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class WeightedPickTests
{
    private sealed class ScriptedRandom : IRandom
    {
        private readonly double[] _seq; private int _i;
        public ScriptedRandom(params double[] seq) { _seq = seq; }
        public double NextDouble() => _seq[_i++ % _seq.Length];
        public int Next(int maxExclusive) => (int)(NextDouble() * maxExclusive);
    }

    [Test]
    public void Picks_first_band_when_rng_low()
    {
        var items = new[] { "a", "b", "c" };
        var weights = new[] { 0.5, 0.3, 0.2 };
        var rng = new ScriptedRandom(0.1);

        var picked = WeightedPick.Pick(rng, items, weights);

        Assert.That(picked, Is.EqualTo("a"));
    }

    [Test]
    public void Picks_middle_band()
    {
        var items = new[] { "a", "b", "c" };
        var weights = new[] { 0.5, 0.3, 0.2 };
        var rng = new ScriptedRandom(0.7);

        var picked = WeightedPick.Pick(rng, items, weights);

        Assert.That(picked, Is.EqualTo("b"));
    }

    [Test]
    public void Renormalizes_when_weights_dont_sum_to_one()
    {
        var items = new[] { "a", "b" };
        var weights = new[] { 50.0, 50.0 };
        var rng = new ScriptedRandom(0.4);

        var picked = WeightedPick.Pick(rng, items, weights);

        Assert.That(picked, Is.EqualTo("a"));
    }

    [Test]
    public void Falls_through_to_last_item_when_rng_exceeds_sum_minus_epsilon()
    {
        var items = new[] { "a", "b" };
        var weights = new[] { 0.5, 0.5 };
        var rng = new ScriptedRandom(0.999);

        var picked = WeightedPick.Pick(rng, items, weights);

        Assert.That(picked, Is.EqualTo("b"));
    }
}
