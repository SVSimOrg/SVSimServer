using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_discard : Skill_discard
{
	public NetworkSkill_discard(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		if (callParameter.targetCards.Count() > 0)
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnDiscard(callParameter.targetCards.ToList());
		}
		return base.Start(callParameter);
	}
}
