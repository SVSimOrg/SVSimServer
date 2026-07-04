using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_independent : Skill_independent
{
	public NetworkSkill_independent(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(parameter.targetCards.ToList());
		return result;
	}
}
