using Wizard.Battle.View.Vfx;

public class NetworkSkill_rush : Skill_rush
{
	public NetworkSkill_rush(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(_targetCards, updateAttackEffect: true, useRecordAttackEffect: true);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, _targetCards);
		return result;
	}
}
