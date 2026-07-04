using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_attack_count : Skill_attack_count
{
	public NetworkSkill_attack_count(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(parameter.targetCards.ToList(), updateAttackEffect: true);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}

	protected override void CallOnChangeMaxAttackableCount(List<BattleCardBase> targetCards)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnChangeMaxAttackableCount(base.SkillPrm.ownerCard, targetCards, GetSetAttackCount());
	}
}
