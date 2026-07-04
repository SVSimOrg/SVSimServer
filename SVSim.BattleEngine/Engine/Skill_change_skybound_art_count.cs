using Wizard.Battle.View.Vfx;

public class Skill_change_skybound_art_count : SkillBase
{

	public Skill_change_skybound_art_count(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_skybound_art_count, -1);
		VfxWithLoadingSequential result = VfxWithLoadingSequential.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
			{
				targetCard.SkillApplyInformation.GiveSkyboundArtCount(new SkyboundArtCountAddModifier(-num));
			}
		}
		return result;
	}
}
