using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_geton : SkillBase
{
	public Skill_geton(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		List<BattleCardBase> list = parameter.targetCards.Where((BattleCardBase t) => !t.IsDead).ToList();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase item in list)
		{
			item.FlagCardAsDestroyedBySkill();
			parallelVfxPlayer.Register(item.SelfBattlePlayer.CardManagement(item, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.GETON, base.UsedRandom, null, base.SkillPrm.ownerCard, this));
			CardParameter baseParameter = item.BaseParameter;
			BuffInfo buffInfo = new BuffInfo(baseParameter.CardId, baseParameter.NormalCardId, this);
			buffInfo.IsGetonSkill = true;
			buffInfo.TargetCard = item;
			base.SkillPrm.ownerCard.AddBuffInfo(buffInfo);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillGetOn(base.SkillPrm.ownerCard, list.Where((BattleCardBase c) => c.IsDead).ToList());
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, list, isFollowInHand: false, addToLastOperation: true));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}
}
