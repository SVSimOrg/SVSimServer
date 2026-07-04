// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.Battle.Touch\SelectCardProcessor.cs
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
namespace Wizard.Battle.Touch
{
public partial class SelectCardProcessor
{
        public SelectCardProcessor(BattleManagerBase battleMgr, BattleCardBase actCard, InputMgr inputMgr, bool isPressCard) { }
        public VfxBase Start() => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public VfxBase Update(float dt, Camera camera) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public virtual VfxWith<ITouchProcessor> End() => default!;
        public virtual bool CheckIsEnd() => default!;
}
}
