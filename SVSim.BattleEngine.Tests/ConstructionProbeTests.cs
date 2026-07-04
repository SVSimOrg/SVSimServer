using System;
using NUnit.Framework;

namespace SVSim.BattleEngine.Tests
{
    // M2 probe (go/no-go step 1): can BattleManagerBase / the two-player pair be constructed
    // HEADLESS at all? This drives the real practice init path
    // (`new SingleBattleMgr(StandardBattleMgrContentsCreator)`), which internally builds the
    // BattlePlayer + BattleEnemy pair, against the M1 shim — with NO Unity runtime.
    //
    // The point of this test is diagnostic: if construction throws, the stack trace tells us the
    // first shim gap on the *resolution* path (vs the compile path M1 already proved). We assert
    // success, but a failure here is the informative outcome we want surfaced.
    [TestFixture]
    public class ConstructionProbeTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        [Test]
        public void SingleBattleMgr_constructs_headless()
        {
            // Mirror the forecast flags the design pins (DP4 / §3): suppress VFX registration and
            // collapse wait delays. TestBattleScope already sets ctx.IsForecast=true; this line is a
            // belt-and-suspenders write through the ambient setter.
            BattleManagerBase.IsForecast = true;

            SingleBattleMgr mgr = null;
            try
            {
                mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            }
            catch (Exception ex)
            {
                Assert.Fail(
                    "Headless construction of SingleBattleMgr threw — first shim gap on the " +
                    "resolution path:\n" + ex);
            }

            Assert.That(mgr, Is.Not.Null);
            Assert.That(mgr.BattlePlayer, Is.Not.Null, "BattlePlayer (self) not created");
            Assert.That(mgr.BattleEnemy, Is.Not.Null, "BattleEnemy (opponent) not created");
        }
    }
}
