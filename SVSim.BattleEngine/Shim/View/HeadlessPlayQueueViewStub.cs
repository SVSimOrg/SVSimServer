// AUTHORED SHIM (not copied). A non-null no-op PlayQueueViewBase the headless RECEIVE-conductor play
// path needs.
//
// On the receive play path OperateMgr.InitSetCard (OperateMgr.cs:201/203/219/221) reads
// BattleView.PlayQueueView and calls AddCardToViewVfx — a pure presentation-layer "card slides into the
// play queue" animation. The m1_stub_gen generated IBattlePlayerView.PlayQueueView getter returns
// default! (null), so the call NREs. The direct-ActionProcessor solo oracles never hit this
// (InitSetCard is on the OperateMgr path, which they bypass). Seed a single shared no-op
// PlayQueueViewBase whose VFX-producing methods return NullVfx and whose abstract geometry members are
// inert. Built through the parameterless base ctor (the BattleCamera ctor eagerly computes screen
// corners off a live camera — skipped). Nothing here touches game state: the authoritative play
// mutation runs in PlayHandCardReflection.Play, not in this view.

using UnityEngine;
using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 7 of 8 methods unrun in baseline
//   Type: Wizard.Battle.View.HeadlessPlayQueueViewStub
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.View
{
    internal sealed class HeadlessPlayQueueViewStub : PlayQueueViewBase
    {
        // Shared instance the generated IBattlePlayerView.PlayQueueView getters return headless.
        public static readonly HeadlessPlayQueueViewStub Instance = new HeadlessPlayQueueViewStub();

        public HeadlessPlayQueueViewStub() : base() { }

        protected override BattlePlayerBase BattlePlayerBase => null;
        protected override float RotationAmount => 0f;
        protected override Vector3 ScreenTopCornerPosition => Vector3.zero;
        protected override Vector3 ScreenBottomCornerPosition => Vector3.zero;

        public override VfxBase AddCardToViewVfx(IBattleCardView playedCardView, bool forceCardIntoPlayQueue, bool isSelectTarget, bool isChoice, bool isChoiceBrave = false)
            => NullVfx.GetInstance();

        public override VfxBase InstantAddCardToViewVfx(IBattleCardView playedCardView, bool forceCardIntoPlayQueue, bool isChoice)
            => NullVfx.GetInstance();

        protected override Vector3 GetScreenTopCornerOffset(float aspectRatio) => Vector3.zero;
        protected override Vector3 GetScreenBottomCornerOffset(float aspectRatio) => Vector3.zero;
    }
}
