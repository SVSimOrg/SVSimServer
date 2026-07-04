namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>RNG seam for testable draw logic. Same contract as <see cref="System.Random"/>.</summary>
public interface IRandom
{
    /// <summary>Returns a value in [0.0, 1.0).</summary>
    double NextDouble();
    /// <summary>Returns a value in [0, maxExclusive).</summary>
    int Next(int maxExclusive);
}
