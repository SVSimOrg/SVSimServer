namespace SVSim.EmulatedEntrypoint.Services;

public class SystemRandom : IRandom
{
    private readonly Random _rng;
    public SystemRandom() { _rng = new Random(); }
    public SystemRandom(int seed) { _rng = new Random(seed); }
    public double NextDouble() => _rng.NextDouble();
    public int Next(int maxExclusive) => _rng.Next(maxExclusive);
}
