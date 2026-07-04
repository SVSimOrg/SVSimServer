using System.Collections.Generic;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_cant_play : SkillBase
{
	private class CantPlayBuffInfoContainer : BuffInfoContainer
	{
		public BattleCardBase Card { get; private set; }

		public CantPlayCardFilterInfo SkillInfo { get; private set; }

		public CantPlayBuffInfoContainer(BattleCardBase card, BuffInfo info, CantPlayCardFilterInfo skillInfo)
			: base(card, info, -1, "", null, 0L)
		{
			Card = card;
			SkillInfo = skillInfo;
		}
	}

	public override bool IsTargetIndicate => false;

	protected override bool IsBattleLog
	{
		get
		{
			if (base.IsBattleLog)
			{
				return !base.SkillPrm.selfBattlePlayer.Class.IsDead;
			}
			return false;
		}
	}

	public Skill_cant_play(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			CantPlayCardFilterInfo cantPlayCardFilterInfo = new CantPlayCardFilterInfo(targetCard, this);
			VfxBase vfx = targetCard.SkillApplyInformation.GiveCantPlay(cantPlayCardFilterInfo);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			if (targetCard.IsClass)
			{
				UpdateClassBuffIfActive(targetCard);
			}
			CantPlayBuffInfoContainer cantPlayBuffInfoContainer = new CantPlayBuffInfoContainer(battleCardBase, buffInfo, cantPlayCardFilterInfo);
			buffInfoContainer.Add(cantPlayBuffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, cantPlayBuffInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.Add(base.SkillPrm.selfBattlePlayer.Class);
			BattleLogManager.GetInstance().AddLogSkillGain(list, this, SkillGainType.CantPlayUnit);
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
		foreach (CantPlayBuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item.Card.SkillApplyInformation.DepriveCantPlay(item.SkillInfo);
			item.Card.RemoveBuffInfo(item._buffInfo);
			if (item._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(item._targetCard);
			}
			parallelVfxPlayer.Register(vfx);
		}
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveCantPlay();
		};
	}
}
