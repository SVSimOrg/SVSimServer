using System;

namespace SVSim.BattleEngine.Rng
{
    // The ONE place engine roll-logic is re-authored (the virtual-override seam restates it rather than
    // body-patching the Engine file). Isolated here so it is unit-testable and pinned to the verbatim
    // engine by the parity test (RngSeamTests.Default_source_matches_engine_generator_and_formula). Mirrors
    // BattleManagerBase.StableRandom: `(int)Math.Floor((double)val * unit)`.
    public static class RandomSourceBridge
    {
        public static int Range(int val, double unit) => (int)Math.Floor((double)val * unit);
    }
}
