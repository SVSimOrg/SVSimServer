// AUTHORED SHIM (not copied). Re-attaches ITouchProcessor to the touch-processor
// stubs. m1_baseclauses.py drops interfaces from recovered base clauses (to avoid
// CS0535), but copied battle code converts these processors to ITouchProcessor
// (e.g. TouchProcessorStack pushes them). Touch input is not on the headless
// resolution path — these are compile-only ballast, so the members are no-ops.
using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View.Vfx;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.Resource;

namespace Wizard.Battle.Touch
{
    // Generated full-surface stubs already carry Start/Update/End/CheckIsEnd —
    // only the dropped interface needs re-declaring.
    public partial class CardTouchProcessorBase : ITouchProcessor { }
    public partial class ChoiceTouchProcessor : ITouchProcessor { }
    public partial class DeckTouchProcessor : ITouchProcessor { }
    public partial class EvolutionTouchProcessor : ITouchProcessor { }
    public partial class SelectCardProcessor : ITouchProcessor { }

    // Empty hand stubs: supply the four ITouchProcessor members as no-ops.
    public partial class SkillTargetSelectTouchProcessor : ITouchProcessor
    {
        public static SkillTargetSelectTouchProcessor Create(BattleManagerBase battleMgr, BattleCardBase actCard, List<SkillBase> selectSkills, Prediction prediction, List<BattleCardBase> selectCards, bool isEvolve, bool isChoiceBrave, BattleCardBase transformCard = null, Action onCompleteLastProcess = null, Action onCancelLastProcess = null) => default!;
        public VfxBase Start() => NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public bool CheckIsEnd() => default!;
    }
    public partial class SetCardProcessor : ITouchProcessor
    {
        public SetCardProcessor(BattleManagerBase battleMgr, BattleCardBase actCard, List<SkillBase> selectSkills, Prediction prediction, Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> getSkillTargetSelectTouchProcessorFunc) { }
        public VfxBase Start() => NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public bool CheckIsEnd() => default!;
    }
    public partial class EvolutionSimpleProcessor : ITouchProcessor
    {
        public EvolutionSimpleProcessor(BattleManagerBase battleMgr, BattleCardBase card, InputMgr inputMgr, List<SkillBase> selectSkills, Func<BattleCardBase, List<BattleCardBase>, List<SkillBase>, bool, SkillTargetSelectTouchProcessor> getSkillTargetSelectTouchProcessorFunc = null, Func<VfxBase> onStartEvolveSkillTargetSelect = null) { }
        public VfxBase Start() => NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public bool CheckIsEnd() => default!;
    }
    public partial class EmotionTouchProcessor : ITouchProcessor
    {
        public EmotionTouchProcessor(IPlayerEmotion emotion, IBattleResourceMgr resourceMgr, TouchControl touchControl, InputMgr inputMgr, VfxMgr emotionVfxMgr, bool isKeyboard = false) { }
        public VfxBase Start() => NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public bool CheckIsEnd() => default!;
    }
    public partial class ClassBuffTouchProcessor : ITouchProcessor
    {
        public ClassBuffTouchProcessor(BattleManagerBase battleMgr, BattleCardBase touchClass, InputMgr inputMgr) { }
        public VfxBase Start() => NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public bool CheckIsEnd() => default!;
    }
    // Decomp: DetailPanelTouchProcessor : CardTouchProcessorBase; the hand stub omits
    // the base, so supply the interface members directly like the other empty stubs.
    public partial class DetailPanelTouchProcessor : ITouchProcessor
    {
        public DetailPanelTouchProcessor(BattleManagerBase battleMgr, BattleCardBase touchCard, InputMgr inputMgr, Prediction prediction, EvolutionSimpleProcessor evolutionProcessor) { }
        public void StopAttackTarget() { }
        public VfxBase Start() => NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => NullVfx.GetInstance();
        public VfxWith<ITouchProcessor> End() => default!;
        public bool CheckIsEnd() => default!;
    }
}
