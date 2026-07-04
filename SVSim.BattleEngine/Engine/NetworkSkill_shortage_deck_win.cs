using Wizard.Battle.View.Vfx;

public class NetworkSkill_shortage_deck_win : Skill_shortage_deck_win
{
	public NetworkSkill_shortage_deck_win(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnAttachShortageDeckWin(base.SkillPrm.ownerCard);
		return result;
	}
}
