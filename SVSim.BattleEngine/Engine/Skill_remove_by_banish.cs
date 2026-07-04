using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class Skill_remove_by_banish : SkillBase
{
	public Skill_remove_by_banish(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			BattleCardBase battleCardBase = list[i];
			buffInfoContainer.Add(new BuffInfoContainer(battleCardBase, null, -1, "", null, 0L));
			battleCardBase.SkillApplyInformation.GiveRemoveByBanish();
		}
		return NullVfxWithLoading.GetInstance();
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			buffInfoContainer[i]._targetCard.SkillApplyInformation.DepriveRemoveByBanish();
		}
		buffInfoContainer.Clear();
		return NullVfxWithLoading.GetInstance();
	}
}
