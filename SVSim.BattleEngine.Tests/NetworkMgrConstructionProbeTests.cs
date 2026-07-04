using NUnit.Framework;
using SVSim.BattleEngine.Rng;
using Wizard.BattleMgr;

namespace SVSim.BattleEngine.Tests
{
    // M13 step 1 (the M2 ConstructionProbe pattern): can a NetworkBattleManagerBase-derived mgr be
    // built headless at all? NetworkBattleManagerSetup constructs NetworkTouchControl(this,
    // _battleCamera, _backGround) + RegisterActionManager + OperateReceive — the largest new shim
    // surface since M5's prefab path. Isolate "ctor runs" before any play is driven.
    [TestFixture]
    public class NetworkMgrConstructionProbeTests : NetworkEmitFixtureBase
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        [Test]
        public void HeadlessNetworkBattleMgr_constructs_headless()
        {
            Assert.DoesNotThrow(() =>
            {
                var mgr = HeadlessEngineEnv.NewSeededHeadlessNetworkBattleMgr();
                Assert.That(mgr, Is.Not.Null);
            });
        }

        [Test]
        public void OnEmit_capture_seam_is_wired_via_injected_agent()
        {
            var (mgr, emitted) = HeadlessEngineEnv.NewNetworkEmitBattle();
            Assert.That(mgr, Is.Not.Null);
            Assert.That(mgr.InstanceNetworkAgent, Is.Not.Null,
                "agent must be injected so NetworkBattleSender's _battleMgr.InstanceNetworkAgent.* calls resolve");
            Assert.That(emitted, Is.Empty, "no emit yet — only the seam is wired");
        }
    }
}
