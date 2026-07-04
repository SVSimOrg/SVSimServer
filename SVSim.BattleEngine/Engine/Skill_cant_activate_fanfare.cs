using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_cant_activate_fanfare : SkillBase
{
	public Skill_cant_activate_fanfare(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "all");
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			VfxBase vfx = targetCard.SkillApplyInformation.GiveCantActivateFanfare(text);
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			if (targetCard.IsClass)
			{
				UpdateClassBuffIfActive(targetCard);
			}
			BuffInfoContainer buffInfoContainer = new BuffInfoContainer(battleCardBase, buffInfo, -1, text, null, 0L);
			base.buffInfoContainer.Add(buffInfoContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoContainer);
			parallelVfxPlayer.Register(vfx);
		}
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
		}));
		if (IsBattleLog && parameter.targetCards.Count() > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, SkillGainType.CantActivateFanfare);
		}
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(vfxWithLoading.MainVfx, parallelVfxPlayer, base.SkillPrm.selfBattlePlayer.UpdateHandCardsCost(), base.SkillPrm.opponentBattlePlayer.UpdateHandCardsCost());
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
			VfxBase vfx = buffInfoContainer[i]._targetCard.SkillApplyInformation.DepriveCantActivateFanfare(buffInfoContainer[i]._stringValue);
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
		parallelVfxPlayer.Register(base.SkillPrm.selfBattlePlayer.UpdateHandCardsCost());
		parallelVfxPlayer.Register(base.SkillPrm.opponentBattlePlayer.UpdateHandCardsCost());
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		string option_type = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "all");
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveCantActivateFanfare(option_type);
		};
	}
}
