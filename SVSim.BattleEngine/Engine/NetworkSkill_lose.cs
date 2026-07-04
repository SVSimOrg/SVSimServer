using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_lose : Skill_lose
{
	public NetworkSkill_lose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(parameter.targetCards.ToList());
		if (_ability == SkillFilterCreator.ContentKeyword.guard.ToString())
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
			return result;
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnLoseSkill(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}
}
