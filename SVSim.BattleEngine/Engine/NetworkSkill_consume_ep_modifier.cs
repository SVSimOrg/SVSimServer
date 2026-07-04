using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_consume_ep_modifier : Skill_consume_ep_modifier
{
	public NetworkSkill_consume_ep_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}
}
