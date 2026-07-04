// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.Touch\ChoiceTouchProcessor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;
namespace Wizard.Battle.Touch
{
public partial class ChoiceTouchProcessor
{
        public ChoiceTouchProcessor(BattleManagerBase battleMgr, BattleCardBase actCard, Prediction prediction, List<SkillBase> choiceSkills, bool isEvolve, bool isChoiceBrave, BattleCardBase accelerateCard = null) { }
        public VfxBase Start() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public virtual VfxBase Update(float dt, Camera camera) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public virtual VfxWith<ITouchProcessor> End() => default!;
        public virtual bool CheckIsEnd() => default!;
}
}
