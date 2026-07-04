using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_metamorphose : Skill_metamorphose
{
	public override bool IsTargetIndicate => false;

	public NetworkSkill_metamorphose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.Where((BattleCardBase c) => c.IsInHand || c.IsInplay).ToList();
		if (list.Count > 0)
		{
			base.SkillPrm.selfBattlePlayer.CallOnMetamorphose(base.SkillPrm.ownerCard, list, _metamorphoseCardId);
		}
		return base.Start(parameter);
	}
}
