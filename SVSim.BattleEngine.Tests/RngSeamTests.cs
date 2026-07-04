using System;
using NUnit.Framework;
using SVSim.BattleEngine.Rng;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    [TestFixture]
    public class RngSeamTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        // RandomSourceBridge.Range must mirror the engine's exact roll arithmetic:
        // BattleManagerBase.StableRandom does `(int)Math.Floor((double)val * unit)`.
        [Test]
        public void Bridge_Range_mirrors_engine_floor_arithmetic()
        {
            Assert.That(RandomSourceBridge.Range(7, 0.0), Is.EqualTo(0));   // floor(7*0)   = 0
            Assert.That(RandomSourceBridge.Range(7, 0.999), Is.EqualTo(6)); // floor(6.993) = 6 (never == val)
            Assert.That(RandomSourceBridge.Range(3, 0.5), Is.EqualTo(1));   // floor(1.5)   = 1 (middle of 3)
            Assert.That(RandomSourceBridge.Range(1, 0.5), Is.EqualTo(0));   // floor(0.5)   = 0
        }

        // SeededRandomSource(seed) must reproduce the engine's own generators EXACTLY: BattleManagerBase
        // seeds both _stableRandom and _stableRandomOnlySelf as `new System.Random(RandomSeed)`
        // (BattleManagerBase.cs:721-722). NextUnit() == synced.NextDouble(); NextSelf(max) == self.Next(max).
        [Test]
        public void SeededSource_reproduces_two_System_Random_streams()
        {
            const int seed = 12345;
            var src = new SeededRandomSource(seed);

            var refSynced = new System.Random(seed); // mirrors _stableRandom
            var refSelf   = new System.Random(seed); // mirrors _stableRandomOnlySelf (separate stream)

            for (int i = 0; i < 8; i++)
                Assert.That(src.NextUnit(), Is.EqualTo(refSynced.NextDouble()), $"NextUnit drift at {i}");
            for (int i = 0; i < 8; i++)
                Assert.That(src.NextSelf(100), Is.EqualTo(refSelf.Next(100)), $"NextSelf drift at {i}");
        }

        // ScriptedRandomSource feeds a known sequence (the oracle's control + the Phase-3 replay seam).
        // It MUST throw on overrun, not wrap: an unexpected extra roll should fail loudly so a test
        // surfaces a miscount of engine RNG calls rather than silently reusing a value.
        [Test]
        public void ScriptedSource_returns_sequence_then_throws_on_overrun()
        {
            var src = new ScriptedRandomSource(new[] { 0.1, 0.5 }, new[] { 3 });

            Assert.That(src.NextUnit(), Is.EqualTo(0.1));
            Assert.That(src.NextUnit(), Is.EqualTo(0.5));
            Assert.That(() => src.NextUnit(), Throws.InvalidOperationException, "should throw on unit overrun");

            Assert.That(src.NextSelf(99), Is.EqualTo(3));
            Assert.That(() => src.NextSelf(99), Throws.InvalidOperationException, "should throw on self overrun");
        }

        // The decoupling (F2): the override must roll REAL values even though IsForecast == true (which
        // forces the un-overridden engine methods to return 0). A ScriptedRandomSource proves the value
        // came from the injected source, not the engine's zeroing.
        [Test]
        public void Override_rolls_real_values_under_IsForecast()
        {
            BattleManagerBase.IsForecast = true; // would zero the un-overridden engine RNG

            // 3 units; with RandomSourceBridge.Range(val, unit) = floor(val*unit):
            //   StableRandom(7) with 0.5  -> floor(3.5) = 3
            //   StableRandomDouble()      -> 0.25
            //   StableRandomOnlySelf(10)  -> scripted self pick 4
            var src = new ScriptedRandomSource(new[] { 0.5, 0.25 }, new[] { 4 });
            var mgr = HeadlessEngineEnv.NewSeededHeadlessBattleMgr(src);

            Assert.That(mgr.StableRandom(7), Is.EqualTo(3), "StableRandom did not use the injected source");
            Assert.That(mgr.randomResult, Is.EqualTo(0.5), "StableRandom must set randomResult to the rolled unit");
            Assert.That(mgr.StableRandomDouble(), Is.EqualTo(0.25), "StableRandomDouble did not use the injected source");
            Assert.That(mgr.randomResult, Is.EqualTo(0.25), "StableRandomDouble must set randomResult");
            Assert.That(mgr.StableRandomOnlySelf(10), Is.EqualTo(4), "StableRandomOnlySelf did not use the injected source");
        }

        // Parity: with the DEFAULT (seeded) source, HeadlessBattleMgr.StableRandom must equal what the
        // verbatim engine would compute — floor(val * new System.Random(seed).NextDouble()) — pinning the
        // re-authored RandomSourceBridge arithmetic to the engine's own formula+generator. (The default
        // source seeds from HeadlessContentsCreator.RandomSeed == 12345.)
        [Test]
        public void Default_source_matches_engine_generator_and_formula()
        {
            BattleManagerBase.IsForecast = true;

            var mgr = HeadlessEngineEnv.NewSeededHeadlessBattleMgr(); // default SeededRandomSource(12345)
            var reference = new System.Random(12345);

            for (int i = 0; i < 10; i++)
            {
                int expected = (int)System.Math.Floor(7 * reference.NextDouble());
                Assert.That(mgr.StableRandom(7), Is.EqualTo(expected), $"parity drift at roll {i}");
            }
        }
    }
}
