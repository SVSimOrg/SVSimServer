using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_sneak : SkillBase
{
	public Skill_sneak(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard2 in parameter.targetCards)
		{
			VfxBase vfx = targetCard2.SkillApplyInformation.GiveSneak();
			BattleCardBase targetCard = targetCard2;
			BuffInfo buff = AddBuffInfoIfNeeded(targetCard2);
			BuffInfoContainer container = new BuffInfoContainer(targetCard, buff, -1, "", null, 0L);
			buffInfoContainer.Add(container);
			SetOnLoseEvent(targetCard, buff, container);
			targetCard.OnAfterAddDamage += delegate
			{
				targetCard.RemoveBuffInfo(buff);
				buffInfoContainer.Remove(container);
				return targetCard.SkillApplyInformation.FourceDepriveSneak();
			};
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.Sneak);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveSneak();
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			parallelVfxPlayer.Register(vfx);
		}
		CallOnUpdateSkillEffect(list);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.FourceDepriveSneak();
		};
	}
}
