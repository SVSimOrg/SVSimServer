// AUTHORED SHIM (not copied) — headless no-op AttackSelectControl for the receive ATTACK path.
//
// The attack conductor dereferences BattlePlayer.BattleView.AttackSelectControl twice on the resolve
// path: InPlayCardReflection.RegisterPairToAttackSelectControl (isPlayer attacks only) calls
// IsCardTranslatable(targetView) + RegisterAttackPair(pair); ActionProcessor.Attack's non-proceeding
// arm calls ResetCardAfterAttack(view). Headless those touch the (UI-only) attack-pair animation state
// — purely cosmetic translate/idle tweening — so a no-op subclass keeps authoritative state intact.
//
// IsCardTranslatable is NOT virtual (it reads cardView.CardInfo.IsClass), so it is left to the BASE
// impl; it resolves correctly headless because the headless BattleCardView's CardInfo is wired to the
// backing card (see Shim/View/ViewUiTouchStubs.cs + Generated/_IfaceImpl.g.cs HEADLESS-FIX). The
// virtual mutating-cosmetic methods are overridden to no-ops here so they never deref the null
// _attackTargetSelectInfo._attackPairsCardIsInvolvedIn animation queue.

using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 3 of 4 methods unrun in baseline
//   Type: Wizard.Battle.View.HeadlessAttackSelectControl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.View
{
    /// <summary>A no-op <see cref="AttackSelectControl"/> seeded as the headless player view's
    /// AttackSelectControl. Overrides only the cosmetic attack-pair animation entry points the receive
    /// ATTACK path invokes; all authoritative damage/death resolution stays in ActionProcessor.Attack.</summary>
    public sealed class HeadlessAttackSelectControl : AttackSelectControl
    {
        public static readonly HeadlessAttackSelectControl Instance = new();

        // The receive path enqueues an attack pair for the translate-up animation. No UI headless, so
        // skip it entirely (the base would deref the card view's null _attackTargetSelectInfo queue).
        public override void RegisterAttackPair(AttackPair attackPair) { }

        // Post-attack card reset is a position tween; no-op headless.
        public override VfxBase ResetCardAfterAttack(IBattleCardView cardToReset) => NullVfx.GetInstance();

        public override VfxBase ResetCardAfterAttackOnReplay() => NullVfx.GetInstance();

        // Idle pingpong tween; no-op headless.
        public override void StartCardIdling(IBattleCardView battleCardView) { }
    }
}
