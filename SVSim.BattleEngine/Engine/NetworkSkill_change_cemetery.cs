using Wizard.Battle.View.Vfx;

public class NetworkSkill_change_cemetery : Skill_change_cemetery
{
	public NetworkSkill_change_cemetery(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		if (base.AddCemeteryCount > 0)
		{
			(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterActionManager.Add(new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.cemetery, base.AddCemeteryCount, base.SkillPrm.selfBattlePlayer.IsPlayer));
		}
		if (base.GainCemeteryCount > 0)
		{
			(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterActionManager.Add(new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.cemetery, -1 * base.GainCemeteryCount, base.SkillPrm.selfBattlePlayer.IsPlayer));
		}
		return result;
	}
}
