namespace SVSim.BattleEngine.Rng
{
    // The battle RNG seam. The headless authoritative mgr (HeadlessBattleMgr) routes the engine's
    // StableRandom* calls through this instead of the IsForecast-gated System.Random fields, so the
    // server rolls real outcomes (decoupling F2) and tests can replay a known sequence.
    public interface IRandomSource
    {
        // Synced stream, [0,1). Drives StableRandom (via RandomSourceBridge.Range) and StableRandomDouble.
        double NextUnit();

        // Self-only stream, [0,max). Mirrors StableRandomOnlySelf (engine: _stableRandomOnlySelf.Next(val)).
        int NextSelf(int max);
    }
}
