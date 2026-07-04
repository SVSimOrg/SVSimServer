using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_chant_count_change : Skill_chant_count_change
{
	public NetworkSkill_chant_count_change(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override VfxBase CheckChantCountDestroy(BattleCardBase card, SkillProcessor skillProcessor)
	{
		if (card.ChantCount <= 0)
		{
			base.SkillPrm.selfBattlePlayer.CallOnSkillDestroyOrBanish(base.SkillPrm.ownerCard);
		}
		return base.CheckChantCountDestroy(card, skillProcessor);
	}

	protected override void CallOnChantCountChange(List<BattleCardBase> targetCards)
	{
		base.SkillPrm.selfBattlePlayer.CallOnChantCountChange(base.SkillPrm.ownerCard, targetCards, (gainChant > 0) ? (gainChant * -1) : addChant);
	}
}
