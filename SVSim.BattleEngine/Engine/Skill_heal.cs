using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_heal : SkillBase
{

	protected List<BattleCardBase.HealResult> HealResultList = new List<BattleCardBase.HealResult>();

	public Skill_heal(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.healing, 0);
		List<BattleCardBase.HealResult> list = new List<BattleCardBase.HealResult>();
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		if (num == -1)
		{
			return NullVfxWithLoading.GetInstance();
		}
		for (int i = 0; i < parameter.targetCards.Count(); i++)
		{
			BattleCardBase battleCardBase = parameter.targetCards.ElementAt(i);
			battleCardBase.SelfBattlePlayer.HealingCards.Add(battleCardBase);
			list2.Add(battleCardBase.VirtualClone(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer));
			BattleCardBase.HealParam healParam = new BattleCardBase.HealParam(num, base.SkillPrm.ownerCard, battleCardBase);
			BattleCardBase.HealResult item = battleCardBase.ApplyHealing(healParam, parameter.skillProcessor);
			list.Add(item);
		}
		RegisterHealTriggerSkill(parameter.skillProcessor, parameter.targetCards, list);
		HealResultList = list;
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer3 = ParallelVfxPlayer.Create();
		foreach (BattleCardBase.HealResult item2 in list)
		{
			parallelVfxPlayer.Register(item2.PrehealVfxVfx);
			parallelVfxPlayer2.Register(item2.HealVfx);
			parallelVfxPlayer3.Register(item2.PosthealVfxVfx);
			base.SkillPrm.ownerCard.SkillApplyInformation.AddSkillHealValue(item2.HealAmount);
		}
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillHeal(list2, list, this);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(parallelVfxPlayer, vfxWithLoading.MainVfx, parallelVfxPlayer2, parallelVfxPlayer3);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		return vfxWithLoadingSequential;
	}

	private void RegisterHealTriggerSkill(SkillProcessor skillProcessor, IEnumerable<BattleCardBase> target, List<BattleCardBase.HealResult> healResult)
	{
		for (int i = 0; i < target.Count(); i++)
		{
			BattleCardBase battleCardBase = target.ElementAt(i);
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(battleCardBase.SelfBattlePlayer, battleCardBase.OpponentBattlePlayer);
			skillProcessor.Register(battleCardBase.Skills.CreateWhenHealing(battleCardBase, skillProcessor, playerInfoPair, healResult[i].HealAmount));
		}
		base.SkillPrm.selfBattlePlayer.StartSkillWhenHealingSelfAndOther(target.ToList(), skillProcessor, healResult.Select((BattleCardBase.HealResult h) => h.HealAmount).ToList());
	}
}
