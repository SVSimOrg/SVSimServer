using UnityEngine;
using Wizard.Battle.View.Vfx;

public class Skill_change_super_skybound_art_count : SkillBase
{

	public Skill_change_super_skybound_art_count(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_super_skybound_art_count, -1);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
			{
				targetCard.SkillApplyInformation.GiveSuperSkyboundArtCount(new SuperSkyboundArtCountAddModifier(-num));
			}
			if (targetCard.HasSuperSkyboundArt && (targetCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch))
			{
				GameObject effectGameObject = null;
				vfxWithLoadingSequential.RegisterToLoadingVfx(NullVfx.GetInstance());
				vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
			}
		}
		return vfxWithLoadingSequential;
	}
}
