using NUnit.Framework;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

namespace SVSim.BattleEngine.Tests
{
    // Regression for the Heal-triggered Skill_heal NRE diagnosed 2026-06-07 (bid 799755786270).
    //
    // A follower with a `when_spell_play` Heal trigger fires on a spell play and routes through
    //   Skill_heal.Start → ClassBattleCardBase.ApplyHealing → CreatePullHandInVfx
    //   → HandViewBase.HandUnfocus (HandViewBase.cs:124-131)
    // The base implementation does `_handControl.SetHandState(HandControl.HandState.Unfocus)`.
    // HeadlessHandViewStub.CreateHandControl returns null in headless, so `_handControl` is null
    // and the base method NREs unconditionally — even when the heal amount is 0.
    //
    // The fix overrides HandUnfocus/HandFocus/FocusRearrangeHandHand on the stub to return
    // NullVfx without touching `_handControl`. These are PURE PRESENTATION methods (visual
    // ease-in/ease-out of the hand cards) — no game-state implications — so no-op'ing them
    // headless is safe; the surrounding state mutations in ApplyHealing (HealLife, skill triggers)
    // still run.
    //
    // Pattern parity with the metamorphose-NRE shim fix in ViewUiTouchStubs.cs (BattleCardView.GameObject
    // lazy non-null): production Unity touches that the headless engine must no-op rather than throw.
    [TestFixture]
    public class HeadlessHandViewStubTests
    {
        [Test]
        public void HandUnfocus_does_not_throw_and_returns_non_null_vfx()
        {
            var stub = HeadlessHandViewStub.Instance;

            VfxBase vfx = null;
            Assert.DoesNotThrow(() => vfx = stub.HandUnfocus(),
                "HandUnfocus must no-op headlessly — the live regression (bid 799755786270) crashed " +
                "Skill_heal.Start when a when_spell_play Heal trigger fired with heal:0 because the " +
                "base HandUnfocus dereferences a null _handControl.");
            Assert.That(vfx, Is.Not.Null, "must return a non-null Vfx (caller registers it on a sequential player).");
        }

        [Test]
        public void HandFocus_does_not_throw_and_returns_non_null_vfx()
        {
            var stub = HeadlessHandViewStub.Instance;

            VfxBase vfx = null;
            Assert.DoesNotThrow(() => vfx = stub.HandFocus(),
                "HandFocus is the sister cosmetic touch (called from CreatePullHandOutVfx on the " +
                "OWNER's turn). Same null _handControl, same headless no-op required.");
            Assert.That(vfx, Is.Not.Null);
        }

        [Test]
        public void FocusRearrangeHandHand_does_not_throw_and_returns_non_null_vfx()
        {
            var stub = HeadlessHandViewStub.Instance;

            VfxBase vfx = null;
            Assert.DoesNotThrow(() => vfx = stub.FocusRearrangeHandHand(),
                "FocusRearrangeHandHand reads _handControl.IsHandStateFocus() before dispatching to " +
                "HandFocus or HandUnfocus; the base implementation would NRE on the read.");
            Assert.That(vfx, Is.Not.Null);
        }
    }
}
