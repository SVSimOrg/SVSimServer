using System.Collections.Generic;
using System.Linq;

public class SkillNoDuplicationRandomSelectInOrderFilter : ISkillSelectFilter
{
	private readonly string _randomSelectCount;

	private readonly ISkillParameterSelectFilter _sortFilter;

	private readonly ISkillParameterSelectFilter _duplicationFilter;

	private readonly ISkillCalcFilter _orderFilter;

	private string _assertionHeadMessage = "id_no_duplication_random_count_in_order=";

	public SkillNoDuplicationRandomSelectInOrderFilter(string optionText, SkillBase skill)
	{
		string[] array = optionText.Split(':');
		_assertionHeadMessage += optionText;
		_duplicationFilter = CheckParamFilter(array[0], skill);
		_sortFilter = CheckParamFilter(array[1], skill);
		switch (array[2])
		{
		case "descending":
			_orderFilter = new SkillCalcMaxFilter();
			break;
		case "ascending":
			_orderFilter = new SkillCalcMinFilter();
			break;
		default:
			_orderFilter = new SkillCalcMaxFilter();
			break;
		}
		_randomSelectCount = array[3];
	}

	public int CalcCount(SkillOptionValue option)
	{
		return option.ParseInt(_randomSelectCount);
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		int num = CalcCount(option);
		List<BattleCardBase> list = new List<BattleCardBase>();
		BattleManagerBase ins = list.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr;
		cards = cards.OrderBy((BattleCardBase x) => x.Index);
		List<BattleCardBase> list2 = cards.ToList();
		bool flag = list.Count < num;
		while (flag && list2.Count > 0)
		{
			int LotteringValue = _orderFilter.Filtering(_sortFilter.Filtering(list2));
			List<BattleCardBase> list3 = list2.Where((BattleCardBase card) => _sortFilter.Filtering(new BattleCardBase[1] { card }).FirstOrDefault() == LotteringValue).ToList();
			while (flag && list3.Count > 0)
			{
				int index = (ins.InstanceIsRandomDraw ? ins.StableRandom(list3.Count()) : 0);
				BattleCardBase battleCardBase = list3[index];
				list.Add(battleCardBase);
				int duplicationValue = _duplicationFilter.Filtering(new BattleCardBase[1] { battleCardBase }).FirstOrDefault();
				list2 = list2.Where((BattleCardBase c) => _duplicationFilter.Filtering(new BattleCardBase[1] { c.Card }).FirstOrDefault() != duplicationValue).ToList();
				list3 = list3.Where((BattleCardBase c) => _duplicationFilter.Filtering(new BattleCardBase[1] { c.Card }).FirstOrDefault() != duplicationValue).ToList();
				flag = list.Count < num;
			}
		}
		return list;
	}

	private ISkillParameterSelectFilter CheckParamFilter(string optionText, SkillBase skill)
	{
		SkillFilterCreator.ParseContentInfo(optionText, out var retParsedInfo);
		ISkillParameterSelectFilter skillParameterSelectFilter = SkillFilterCreator.CreateParameterSelectFilter(retParsedInfo.Name, retParsedInfo, skill);
		if (skillParameterSelectFilter == null)
		{
			skillParameterSelectFilter = new SkillParameterSelectCostFilter();
		}
		return skillParameterSelectFilter;
	}
}
