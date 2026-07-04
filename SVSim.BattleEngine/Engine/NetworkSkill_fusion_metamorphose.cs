using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_fusion_metamorphose : Skill_fusion_metamorphose
{
	public override bool IsTargetIndicate => false;

	public NetworkSkill_fusion_metamorphose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.Where((BattleCardBase c) => c.IsInHand).ToList();
		VfxWithLoading result = base.Start(parameter);
		if (list.Count > 0)
		{
			base.SkillPrm.selfBattlePlayer.CallOnFusionMetamorphose(_metamorphoseCardId);
		}
		return result;
	}
}
