using System.Collections.Generic;
using Wizard.Battle;

public abstract class SkillParameterSelectGenericValueFilter : ISkillParameterSelectFilter
{
	protected string _key = string.Empty;

	protected SkillParameterSelectGenericValueFilter(string key, SkillBase skill)
	{
		_key = key;
		if (_key == string.Empty)
		{
			_key = "0";
		}
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		List<int> list = new List<int>();
		foreach (IReadOnlyBattleCardInfo cardInfo in cardInfos)
		{
			BattleCardBase card = cardInfo as BattleCardBase;
			list.Add(GetGenericValue(card));
		}
		return list;
	}

	protected abstract int GetGenericValue(BattleCardBase card);
}
