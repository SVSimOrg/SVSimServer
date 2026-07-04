using Wizard.Battle.View.Vfx;

public class NetworkSkill_special_lose : Skill_special_lose
{
	public NetworkSkill_special_lose(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading vfxWithLoading = base.Start(parameter);
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			((NetworkBattleManagerBase)SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr)._specialWinVfx = vfxWithLoading;
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSpecialLose(base.SkillPrm.ownerCard);
		return vfxWithLoading;
	}
}
