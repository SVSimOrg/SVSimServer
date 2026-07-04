// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.View\InPlayCardFrameEffectControl.cs
// TODO(engine-cleanup-pass2): 11 of 12 methods unrun in baseline
//   Type: Wizard.Battle.View.InPlayCardFrameEffectControl
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt

using System;
using UnityEngine;
namespace Wizard.Battle.View
{
public partial class InPlayCardFrameEffectControl
{
        public InPlayCardFrameEffectControl(Func<CardTemplate> getCardTemplateFunc, Func<bool> getIsAbleToAttackFunc, Func<bool> getIsUnableToAttackClassFunc) { }
        public virtual void UpdateCanAttackEffect(Func<bool> isUnableToAttackClassFunc = null, bool isSelfTurn = true) { }
        public virtual void SetIsSelectingAttackTarget(bool enable) { }
        public virtual void HideFrameEffect() { }
}
}
