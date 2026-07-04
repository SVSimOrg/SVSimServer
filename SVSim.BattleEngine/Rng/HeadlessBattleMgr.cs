using Wizard.BattleMgr;

namespace SVSim.BattleEngine.Rng
{
    // The headless authoritative single-battle mgr. Overrides the three BattleManagerBase RNG methods
    // (now virtual per the Task-4 DP5 patch) to delegate to an injected IRandomSource instead of the
    // IsForecast-gated System.Random fields. This is the F2 decoupling: VFX stays suppressed
    // (IsForecast == true) while RNG rolls real. The skill RNG path calls BattleManagerBase.GetIns()
    // .StableRandom*, and the base ctor registers `this` as the singleton, so constructing the battle as
    // HeadlessBattleMgr makes every roll dispatch (virtually) to these overrides.
    //
    // randomResult is set inside the overrides (it has a protected setter, reachable only from a
    // subclass — NOT from RandomSourceBridge); it is read by the Phase-2 NetworkSkill_cost_change emit
    // path, so the overrides keep it faithful. The arithmetic itself lives in RandomSourceBridge so it
    // stays unit-testable and reusable by a future NetworkBattleManagerBase-derived mgr.
    public sealed class HeadlessBattleMgr : SingleBattleMgr
    {
        private readonly IRandomSource _rng;

        public HeadlessBattleMgr(IBattleMgrContentsCreator contentsCreator, IRandomSource rng = null)
            : base(contentsCreator)
        {
            _rng = rng ?? new SeededRandomSource(contentsCreator.RandomSeed);
        }

        // Phase-5 chunk 45: overload for the pre-seeded GameMgr path — fixtures build a GameMgr,
        // seed it (SeedCharaIds + SeedNetUser), then hand it to this ctor. Retires the ambient bridge.
        public HeadlessBattleMgr(IBattleMgrContentsCreator contentsCreator, IRandomSource rng, GameMgr gameMgr)
            : base(contentsCreator, gameMgr)
        {
            _rng = rng ?? new SeededRandomSource(contentsCreator.RandomSeed);
        }

        // KNOWN DIVERGENCE: the base StableRandom/StableRandomDouble also bump a private
        // `stableRandomCount` diagnostic field; these overrides cannot (it's private to the base) and do
        // not. The field is currently unread anywhere in the engine, so this is harmless; if a future
        // replay/audit path starts reading the count, promote it via a protected accessor (another DP5
        // patch) rather than leaving it silently zero.
        public override int StableRandom(int val)
        {
            double unit = _rng.NextUnit();
            randomResult = unit;
            return RandomSourceBridge.Range(val, unit);
        }

        public override double StableRandomDouble()
        {
            double unit = _rng.NextUnit();
            randomResult = unit;
            return unit;
        }

        public override int StableRandomOnlySelf(int val) => _rng.NextSelf(val);
    }
}
