using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_quick : Skill_quick
{
	public NetworkSkill_quick(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(parameter.targetCards.ToList(), updateAttackEffect: true, useRecordAttackEffect: true);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}
}
