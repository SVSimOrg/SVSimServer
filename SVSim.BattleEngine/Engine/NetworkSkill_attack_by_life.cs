using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_attack_by_life : Skill_attack_by_life
{
	public NetworkSkill_attack_by_life(SkillParameter skillPrm, string option)
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
