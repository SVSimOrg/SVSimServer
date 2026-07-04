// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.Touch\EvolutionTouchProcessor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;
namespace Wizard.Battle.Touch
{
public partial class EvolutionTouchProcessor
{
        public Func<VfxBase> OnAfterEvolveDragSelect;
        public EvolutionTouchProcessor(BattleManagerBase battleMgr, InputMgr inputMgr, IBattleResourceMgr resourceMgr, bool inDetailPanelEvolution, Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> getSkillTargetSelectTouchProcessorFunc, Prediction prediction) { }
        public VfxBase Start() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public virtual VfxBase ShowEvolutionMessage() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public virtual bool CheckIsEnd() => default!;
        public void SetStopSelectFlag() { }
}
}
