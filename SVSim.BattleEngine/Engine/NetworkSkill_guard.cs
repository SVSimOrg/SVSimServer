using Wizard.Battle.View.Vfx;

public class NetworkSkill_guard : Skill_guard
{
	public NetworkSkill_guard(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, _targetCards);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(_targetCards);
		return result;
	}
}
