using System.Collections.Generic;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_cant_summon : SkillBase
{
	public enum CantSummonInfo
	{
		None,
		DeckSelf
	}

	private CantSummonInfo type;

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

	public Skill_cant_summon(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.cant_summon) == "deck_self")
		{
			type = CantSummonInfo.DeckSelf;
		}
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfx = targetCard.SkillApplyInformation.GiveCantSummon(type);
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(targetCard, buffInfo, -1, "", null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(targetCard, buffInfo, buffInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		if (IsBattleLog)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.Add(base.SkillPrm.selfBattlePlayer.Class);
			BattleLogManager.GetInstance().AddLogSkillGain(list, this, SkillGainType.CantSummon);
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
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveCantSummon(type);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
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
			return card.SkillApplyInformation.ForceDepriveCantSummon();
		};
	}
}
