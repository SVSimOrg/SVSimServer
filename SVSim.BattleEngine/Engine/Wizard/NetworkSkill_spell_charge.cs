using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public class NetworkSkill_spell_charge : Skill_spell_charge
{
	public NetworkSkill_spell_charge(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		RegisterSkillEndEvent();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnSpellCharge(base.SkillPrm.ownerCard, _targetCards, _addList);
		return result;
	}

	private void RegisterSkillEndEvent()
	{
		// IsWatchBattle and IsReplayBattle are both const-false in headless (Phase 4).
		if (!base.SkillPrm.ownerCard.IsPlayer && RegisterFilter.IsFilterCard(this))
		{
			return;
		}
		Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> value = delegate(SkillBase localSkill, List<BattleCardBase> localCards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
		{
			if (localCards.Count() > 0)
			{
				RegisterSpellboost data = new RegisterSpellboost(localCards, localSkill, base.AddCount, base.DiffAddCount);
				(base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterActionManager.Add(data);
			}
			return NullVfx.GetInstance();
		};
		base.OnSkillEnd -= value;
		base.OnSkillEnd += value;
	}
}
