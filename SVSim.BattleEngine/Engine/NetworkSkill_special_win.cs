using Wizard.Battle.View.Vfx;

public class NetworkSkill_special_win : Skill_special_win
{
	public NetworkSkill_special_win(SkillParameter skillPrm, string option)
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
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSpecialWin(base.SkillPrm.ownerCard);
		return vfxWithLoading;
	}
}
