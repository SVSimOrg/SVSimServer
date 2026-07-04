using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_cant_activate_shortage_deck_win : SkillBase
{
	public Skill_cant_activate_shortage_deck_win(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfx = targetCard.SkillApplyInformation.GiveCantActivateShortageDeckWin();
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			if (targetCard.IsClass)
			{
				UpdateClassBuffIfActive(targetCard);
			}
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.CantActivateShortageDeckWin);
		}
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(vfxWithLoading.MainVfx, parallelVfxPlayer);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			VfxBase vfx = buffInfoContainer[i]._targetCard.SkillApplyInformation.ForceDepriveCantActivateShortageDeckWin();
			list.Add(buffInfoContainer[i]._targetCard);
			buffInfoContainer[i]._targetCard.RemoveBuffInfo(buffInfoContainer[i]._buffInfo);
			if (buffInfoContainer[i]._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(buffInfoContainer[i]._targetCard);
			}
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
			return card.SkillApplyInformation.ForceDepriveCantActivateShortageDeckWin();
		};
	}
}
