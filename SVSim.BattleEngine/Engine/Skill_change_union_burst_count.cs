using UnityEngine;
using Wizard.Battle.View.Vfx;

public class Skill_change_union_burst_count : SkillBase
{

	public Skill_change_union_burst_count(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_union_burst_count, -1);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			targetCard.SkillApplyInformation.GiveUnionBurstCount(new UnionBurstCountAddModifier(-num));
			if (targetCard.HasUnionBurst && (targetCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch))
			{
				GameObject effectGameObject = null;
				vfxWithLoadingSequential.RegisterToLoadingVfx(NullVfx.GetInstance());
				vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
			}
		}
		return vfxWithLoadingSequential;
	}

	public int GetGainUnionBurstCount()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_union_burst_count, 0);
	}
}
