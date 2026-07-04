using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class ApplySkillTargetFilterCollection : SkillFilterCollectionBase
{
	public List<ISkillCustomSelectFilter> ApplyCustomSelectFilterList { get; set; }

	public List<ISkillExclutionFilter> ApplyExclutionFilterList { get; private set; }

	public ISkillSelectFilter ApplySelectFilter { get; set; }

	public List<ApplySkillTargetFilterCollection> ApplyAndFilter { get; set; }

	public ApplySkillTargetFilterCollection()
	{
		ApplyCustomSelectFilterList = new List<ISkillCustomSelectFilter>();
		ApplyExclutionFilterList = new List<ISkillExclutionFilter>();
		ApplyAndFilter = new List<ApplySkillTargetFilterCollection>();
	}

	public List<IReadOnlyBattleCardInfo> Filtering(BattlePlayerReadOnlyInfoPair pair, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> AndFilterTargets = new List<IReadOnlyBattleCardInfo>();
		IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = null;
		if (ApplyAndFilter.Count <= 0)
		{
			if (base.BattlePlayerFilter != null)
			{
				battlePlayerInfos = base.BattlePlayerFilter.Filtering(pair);
			}
			if (base.TargetFilter != null)
			{
				list = base.TargetFilter.Filtering(battlePlayerInfos, checkerOption).ToList();
				if (false /* Pre-Phase-5b: XorShiftRandom is a mgr-instance feature; guard collapses to false */)
				{
					return list;
				}
			}
			foreach (ISkillCardFilter cardFilter in base.CardFilterList)
			{
				list = cardFilter.Filtering(list, optionValue).ToList();
			}
			int i = 0;
			for (int count = ApplyCustomSelectFilterList.Count; i < count; i++)
			{
				list = ApplyCustomSelectFilterList[i].Filtering(list, battlePlayerInfos, checkerOption).ToList();
			}
			for (int j = 0; j < ApplyExclutionFilterList.Count; j++)
			{
				list = ApplyExclutionFilterList[j].Filtering(list, battlePlayerInfos, checkerOption, optionValue).ToList();
			}
		}
		else
		{
			for (int k = 0; k < ApplyAndFilter.Count; k++)
			{
				List<BattleCardBase> cards = ApplyAndFilter[k].Filtering(pair, checkerOption, optionValue).Cast<BattleCardBase>().ToList();
				List<IReadOnlyBattleCardInfo> collection = (from IReadOnlyBattleCardInfo x in ApplyAndFilter[k].SelectFilter.Filtering(cards, optionValue, checkerOption)
					where !AndFilterTargets.Contains(x)
					select x).ToList();
				AndFilterTargets.AddRange(collection);
			}
		}
		List<IReadOnlyBattleCardInfo> list2 = list.ToList();
		list2.AddRange(AndFilterTargets);
		return list2;
	}

	public bool SimpleFiltering(IReadOnlyBattleCardInfo targetCard, BattlePlayerReadOnlyInfoPair pair, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo> { targetCard };
		IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = base.BattlePlayerFilter.Filtering(pair);
		for (int i = 0; i < base.CardFilterList.Count; i++)
		{
			list = base.CardFilterList[i].Filtering(list, optionValue).ToList();
		}
		for (int j = 0; j < ApplyCustomSelectFilterList.Count; j++)
		{
			list = ApplyCustomSelectFilterList[j].Filtering(list, battlePlayerInfos, checkerOption).ToList();
		}
		for (int k = 0; k < ApplyExclutionFilterList.Count; k++)
		{
			list = ApplyExclutionFilterList[k].Filtering(list, battlePlayerInfos, checkerOption, optionValue).ToList();
		}
		return list.Count() > 0;
	}
}
