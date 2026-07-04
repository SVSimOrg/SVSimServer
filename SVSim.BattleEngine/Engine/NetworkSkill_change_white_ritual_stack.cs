using Wizard.Battle.View.Vfx;

public class NetworkSkill_change_white_ritual_stack : Skill_change_white_ritual_stack
{
	public NetworkSkill_change_white_ritual_stack(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterInplayWhiteRitualStack(base.SkillPrm.selfBattlePlayer);
		return result;
	}

	protected override void CallOnChangeWhiteRitualStack(BattleCardBase target, int changeCount)
	{
		base.SkillPrm.selfBattlePlayer.CallOnChangeWhiteRitualStack(target, changeCount);
	}

	protected override void CallOnSkillDestroy()
	{
		base.SkillPrm.selfBattlePlayer.CallOnSkillDestroyOrBanish(base.SkillPrm.ownerCard);
	}
}
