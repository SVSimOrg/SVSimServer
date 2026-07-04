using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Generic cumulative-band weighted picker used by PackOpenService for tier-by-slot
/// and card-within-tier sampling. Renormalizes weights internally (sums &lt;1 absorb
/// into the last band; sums &gt;1 scale down).
/// </summary>
public static class WeightedPick
{
    public static T Pick<T>(IRandom rng, IReadOnlyList<T> items, IReadOnlyList<double> weights)
    {
        if (items.Count == 0) throw new ArgumentException("WeightedPick: items is empty.");
        if (items.Count != weights.Count) throw new ArgumentException("WeightedPick: items / weights length mismatch.");

        double sum = 0;
        for (int i = 0; i < weights.Count; i++) sum += weights[i];
        if (sum <= 0) return items[rng.Next(items.Count)];

        double r = rng.NextDouble() * sum;
        double cum = 0;
        for (int i = 0; i < items.Count - 1; i++)
        {
            cum += weights[i];
            if (r < cum) return items[i];
        }
        return items[^1];
    }
}
