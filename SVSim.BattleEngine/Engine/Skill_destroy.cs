using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_destroy : SkillBase
{
	protected List<BattleCardBase> TargetList;

	public override bool IsTargetIndicate => false;

	public Skill_destroy(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		TargetList = parameter.targetCards.Where((BattleCardBase s) => !s.IsInDeck).ToList();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase target in TargetList)
		{
			target.FlagCardAsDestroyedBySkill();
			parallelVfxPlayer.Register(target.SelfBattlePlayer.CardManagement(target, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, null, null, this));
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillDeath(TargetList.Where((BattleCardBase c) => c.IsDead).ToList(), this);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, TargetList, isFollowInHand: false, addToLastOperation: true));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}
}
