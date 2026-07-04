// TODO(engine-cleanup-pass2): 1 of 3 methods unrun in baseline
//   Type: SVSim.BattleEngine.Rng.HeadlessNetworkBattleMgr
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

namespace SVSim.BattleEngine.Rng
{
    // The headless authoritative NETWORK battle mgr — the emitting twin of HeadlessBattleMgr. Emission
    // lives on the NetworkBattleSender, and the *override that actually invokes it* (SendPlayCard ->
    // NetworkSender.SendPlayCard) lives on NetworkStandardBattleMgr, NOT NetworkBattleManagerBase: the
    // base SendPlayCard is an empty virtual no-op (NetworkBattleManagerBase.cs:598). NetworkStandardBattleMgr
    // is the production standard-PvP mgr; its ctor also creates the NetworkSender (NetworkStandardBattleMgr.cs:92),
    // so no manual sender wiring is needed here. Extending the base instead would resolve state but never
    // emit (spec §7 risk 2a). RNG overrides are identical to HeadlessBattleMgr (the same BattleManagerBase
    // virtuals + RandomSourceBridge), so the M14 rand-list emit reuses this mgr unchanged. M13's
    // deterministic card never exercises a roll.
    public sealed class HeadlessNetworkBattleMgr : NetworkStandardBattleMgr
    {
        private readonly IRandomSource _rng;

        public HeadlessNetworkBattleMgr(Wizard.BattleMgr.IBattleMgrContentsCreator contentsCreator, IRandomSource rng = null)
            : base(contentsCreator)
        {
            _rng = rng ?? new SeededRandomSource(contentsCreator.RandomSeed);
        }

        // Phase-5 chunk 45: overload for the pre-seeded GameMgr path.
        public HeadlessNetworkBattleMgr(Wizard.BattleMgr.IBattleMgrContentsCreator contentsCreator, IRandomSource rng, GameMgr gameMgr)
            : base(contentsCreator, gameMgr)
        {
            _rng = rng ?? new SeededRandomSource(contentsCreator.RandomSeed);
        }

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
