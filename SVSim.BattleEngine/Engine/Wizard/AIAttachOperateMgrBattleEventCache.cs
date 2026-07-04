using System;
using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public class AIAttachOperateMgrBattleEventCache
{
	public Action<BattleCardBase, BattleCardBase, IEnumerable<BattleCardBase>> OnSetCardSuccessEvent;

	public Func<BattleCardBase, VfxBase> OnSetCardExecutedEvent;

	public Action<BattleCardBase, BattleCardBase, IEnumerable<BattleCardBase>> OnEvolveSuccessEvent;

	public Func<BattleCardBase, VfxBase> OnEvolveCompleteEvent;

	public Func<BattleCardBase, BattleCardBase, SkillProcessor, VfxBase> OnBeforeAttackEvent;

	public Func<BattleCardBase, BattleCardBase, bool, VfxBase> OnAttackExecutedEvent;

	public Action<BattleCardBase, IEnumerable<BattleCardBase>> OnBeforeFusionEvent;

	public Func<BattleCardBase, VfxBase> OnAfterFusionEvent;
}
