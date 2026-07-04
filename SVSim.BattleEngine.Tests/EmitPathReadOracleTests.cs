using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M13 (hub O1, deterministic): the first headless observation of the EMIT path. Drive the proven M3
    // fixed-damage spell (900124030) through mgr.OperateMgr.PlayCard on a NetworkBattleManagerBase-derived
    // mgr and confirm the engine reaches its emission path (RealTimeNetworkAgent.OnEmit fires PlayActions)
    // without crashing, while the committed state still matches the M3 direct-ActionProcessor oracle.
    // Liveness only (E4); structural frame decoding + the RNG rand-list (M14) are deferred.
    [TestFixture]
    public class EmitPathReadOracleTests : NetworkEmitFixtureBase
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        // The process-global reset (IsForecast=true + clear injected agent) now lives in the shared
        // NetworkEmitFixtureBase.ResetNetworkEmitGlobals [TearDown], inherited here — see that file
        // for why the leak matters.

        [Test]
        public void M3_spell_driven_via_OperateMgr_reaches_emit_without_crashing()
        {
            var (mgr, emitted) = HeadlessEngineEnv.NewNetworkEmitBattle();
            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            int leaderLifeBefore = enemy.Class.Life;

            var spell = HeadlessEngineEnv.CreateHeadlessHandCard(
                HeadlessEngineEnv.SpellId, index: 1, isPlayer: true, mgr);
            player.HandCardList.Add(spell);
            int cost = spell.Cost;
            player.Pp = 10;

            Assert.DoesNotThrow(
                () => mgr.OperateMgr.PlayCard(spell, isPlayer: true, selectCards: null),
                "OperateMgr.PlayCard threw driving the M3 spell through the emit path");

            Assert.Multiple(() =>
            {
                // Emit reached: OnEmit fired with PlayActions (the O1 liveness signal).
                Assert.That(emitted, Does.Contain(NetworkBattleDefine.NetworkBattleURI.PlayActions),
                    "the engine did not reach a PlayActions emit");
                // State intact vs the M3 direct-path oracle.
                Assert.That(enemy.Class.Life, Is.EqualTo(leaderLifeBefore - 3), "enemy leader should take 3");
                Assert.That(player.Pp, Is.EqualTo(10 - cost), "PP should be paid");
                Assert.That(player.HandCardList, Does.Not.Contain(spell), "spell should leave the hand");
                Assert.That(player.CemeteryList, Does.Contain(spell), "spell should land in the cemetery");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(spell), "a spell does not occupy the board");
            });

            // Best-effort (F-E-7): with CurrentMatchingStatus seeded non-Disconnected (NewNetworkEmitBattle),
            // the flow reaches stockEmitMessageMgr.StockData(info); read it back. If the stock machinery is
            // not drivable headless this milestone, this assertion is DEFERRED to structural validation
            // (spec §6) — the OnEmit + no-throw + state checks above are the decisive O1 read on their own.
            var agent = mgr.InstanceNetworkAgent; // Phase-5 chunk 41: was Wizard.ToolboxGame.RealTimeNetworkAgent
            var stocked = HeadlessEngineEnv.TryReadStockedEmitData(agent); // returns null if unreachable
            if (stocked != null)
                Assert.That(stocked, Is.Not.Empty, "the emitted dict should be stocked non-empty");
            else
                Assert.Inconclusive("payload-presence DEFERRED: stock-sequencer not drivable headless (spec §6)");
        }
    }
}
