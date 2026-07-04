using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

internal class Skill_repeat_skill : SkillBase
{
	public Skill_repeat_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		string option = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.repeat_type);
		string targetOption = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.target, "unit");
		SkillGainType gainType = SkillGainType.Null;
		if (IsBattleLog)
		{
			string option2 = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.repeat_type, "_OPT_NULL_");
			gainType = ((option2 != null && option2 == "when_destroy") ? SkillGainType.RepeatLastWord : SkillGainType.Null);
		}
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (targetCard is ClassBattleCardBase && !targetCard.SkillApplyInformation.RepeatSkillTimingList.Any((RepeatSkillInfo s) => s.Timing == option && s.Target == targetOption))
			{
				vfxWithLoadingSequential.RegisterToMainVfx(targetCard.SkillApplyInformation.GiveRepeatSkill(option, targetOption, this));
				CardParameter baseParameter = base.SkillPrm.ownerCard.BaseParameter;
				BuffInfo buffInfo = new BuffInfo(baseParameter.BaseCardId, baseParameter.NormalCardId, this);
				targetCard.AddBuffInfo(buffInfo);
				if (targetCard.IsClass)
				{
					UpdateClassBuffIfActive(targetCard);
				}
				BuffInfoContainer item = new BuffInfoContainer(targetCard, buffInfo, -1, "", null, 0L);
				buffInfoContainer.Add(item);
			}
		}
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		if (IsBattleLog && !base.SkillPrm.selfBattlePlayer.Class.IsDead && base.SkillPrm.ownerCard.SelfBattlePlayer.Class.SkillApplyInformation.RepeatSkillTimingList.Where((RepeatSkillInfo s) => s.Timing == option).Count() <= 1)
		{
			BattleLogManager.GetInstance().AddLogSkillGain(parameter.targetCards.ToList(), this, gainType);
		}
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			if (item._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(item._targetCard);
			}
		}
		return NullVfxWithLoading.GetInstance();
	}

	public static bool CheckCardType(string cardType, BattleCardBase card)
	{
		if ((cardType == "unit" && card.IsUnit) || (cardType == "field" && card.IsField))
		{
			return true;
		}
		return false;
	}
}
