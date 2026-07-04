// AUTHORED SHIM (not copied). A non-null no-op HandViewBase the headless emit path needs.
//
// On the OperateMgr/network play path, BattlePlayerBase.SetupActionProcessorEvent subscribes an
// OnBeforePlayCard handler that calls BattleView.HandView.RemoveCardFromView (BattlePlayerBase.cs:1422) —
// a pure presentation-layer hand-card removal. The m1_stub_gen generated IBattlePlayerView.HandView getter
// returns default! (null), so the call NREs. The direct-ActionProcessor solo oracles never hit this
// (SetupActionProcessorEvent is OperateMgr-only). Seed a single shared no-op HandViewBase whose card-view
// list is non-null so RemoveCardFromView is a safe no-op (the played card is never in this view's list, so
// the abstract RearrangeHand is never reached). Nothing here touches game state.

using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 1 of 5 methods unrun in baseline
//   Type: Wizard.Battle.View.HeadlessHandViewStub
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.View
{
    internal sealed class HeadlessHandViewStub : HandViewBase
    {
        // Shared instance the generated IBattlePlayerView.HandView getters return headless.
        public static readonly HeadlessHandViewStub Instance = new HeadlessHandViewStub();

        // Base param ctor initializes the protected _battleCardViewList (the default ctor leaves it null,
        // which RemoveCardFromView would NRE on). CreateHandControl is overridden to a null no-op below.
        public HeadlessHandViewStub() : base(null, null) { }

        protected override void RearrangeHand(float rearrangeTime, bool isNewReplayMoveTurn = false) { }

        protected override HandControl CreateHandControl(GameObject handGameObject, BattleCamera battleCamera) => null;

        // HEADLESS-FIX: with CreateHandControl returning null, the base implementations of
        // HandUnfocus/HandFocus/FocusRearrangeHandHand (HandViewBase.cs:124/133/142) NRE on
        // `_handControl.SetHandState(...)`. These are PURE PRESENTATION methods — they ease the hand
        // cards in/out visually as a side effect of leader healing, spell selection, etc. — with no
        // game-state implications, so the safe headless behavior is a no-op returning NullVfx.
        //
        // Live regression: bid 799755786270 (2026-06-07). A follower with a `when_spell_play` Heal
        // trigger fired on its leader for 0 (the trigger fires regardless of heal amount, and even a
        // 0-heal still drives `ApplyHealing` → `CreatePullHandInVfx` → `HandView.HandUnfocus()`
        // unconditionally per ClassBattleCardBase.cs:234/239). Stack:
        //   Skill_heal.Start → ClassBattleCardBase.ApplyHealing → CreatePullHandInVfx
        //   → HandViewBase.HandUnfocus → NRE on null _handControl.
        // Same pattern as the metamorphose-NRE shim fix (ViewUiTouchStubs.cs's
        // BattleCardView.GameObject lazy non-null): production Unity touches that the headless
        // engine needs to no-op rather than throw.
        public override VfxBase HandUnfocus() => NullVfx.GetInstance();
        public override VfxBase HandFocus() => NullVfx.GetInstance();
        public override VfxBase FocusRearrangeHandHand() => NullVfx.GetInstance();
    }
}
