using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_copy_skill : Skill_copy_skill
{
	public NetworkSkill_copy_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		bool updateAttackEffect = SkillType == "quick" || SkillType == "rush";
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(new List<BattleCardBase> { base.SkillPrm.ownerCard }, updateAttackEffect);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(parameter.targetCards.ToList(), updateAttackEffect);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}
}
