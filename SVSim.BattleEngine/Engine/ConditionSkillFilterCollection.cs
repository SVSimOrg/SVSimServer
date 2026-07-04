using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;

public class ConditionSkillFilterCollection : SkillFilterCollectionBase
{

	public List<ISkillConditionChecker> ConditionCheckerFilterList { get; set; }

	public List<SkillVariableComareFilter> VariableCompareFilter { get; set; }

	public List<SkillAnyConditionFilter> AnyConditionFilter { get; set; }

	public ConditionSkillFilterCollection()
	{
		ConditionCheckerFilterList = new List<ISkillConditionChecker>(8);
		VariableCompareFilter = new List<SkillVariableComareFilter>();
		AnyConditionFilter = new List<SkillAnyConditionFilter>();
	}

	public bool Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair, BattleCardBase ownerCard, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue, bool isPrePlay, SkillBase skill, bool isSkipTargetAiSelect = false)
	{
		SkillCollectionBase.SetupOptionValue(optionValue, playerInfoPair, ownerCard, skill, checkerOption, isPrePlay);
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		if (VariableCompareFilter.Count() != 0)
		{
			bool flag5 = VariableCompareFilter.All((SkillVariableComareFilter s) => s.Filtering(optionValue));
			if (isSkipTargetAiSelect && VariableCompareFilter.FirstOrDefault().Lhs.Contains("hand_other_self") && ownerCard.SelfBattlePlayer.HandCardList.Count > 0)
			{
				flag5 = true;
			}
			bool flag6 = ConditionCheckerFilterList.Where((ISkillConditionChecker f) => f is SkillPreprocessBase).All((ISkillConditionChecker f) => f.IsRight(playerInfoPair, checkerOption));
			flag = flag5 && flag6;
		}
		Func<ISkillConditionChecker, Func<BattlePlayerReadOnlyInfoPair, SkillConditionCheckerOption, bool, bool>> checkRightFunc;
		if (isPrePlay)
		{
			checkRightFunc = (ISkillConditionChecker f) => f.IsRightPrePlay;
		}
		else
		{
			checkRightFunc = (ISkillConditionChecker f) => f.IsRight;
		}
		if (ConditionCheckerFilterList.Count() != 0)
		{
			flag2 = ConditionCheckerFilterList.All((ISkillConditionChecker c) => checkRightFunc(c)(playerInfoPair, checkerOption, arg3: false));
		}
		if (AnyConditionFilter.Count > 0)
		{
			flag3 = AnyConditionFilter.All((SkillAnyConditionFilter c) => c.Filtering(playerInfoPair, ownerCard, checkerOption, optionValue, isPrePlay, skill, isSkipTargetAiSelect));
		}
		if (base.BattlePlayerFilter != null)
		{
			flag4 = FilteringBase(playerInfoPair, checkerOption, optionValue, isSkipTargetAiSelect).Any();
		}
		return flag && flag2 && flag4 && flag3;
	}
}
