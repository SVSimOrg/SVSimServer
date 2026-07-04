using System;

namespace SVSim.BattleEngine.Rng
{
    // Default source. Faithfully reproduces the engine's own RNG: BattleManagerBase seeds two separate
    // System.Random(RandomSeed) instances (_stableRandom synced, _stableRandomOnlySelf self-only) at
    // BattleManagerBase.cs:721-722. The authoritative server uses this; tests use ScriptedRandomSource.
    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random _synced;
        private readonly Random _self;

        public SeededRandomSource(int seed)
        {
            _synced = new Random(seed);
            _self = new Random(seed);
        }

        public double NextUnit() => _synced.NextDouble();
        public int NextSelf(int max) => _self.Next(max);
    }
}
