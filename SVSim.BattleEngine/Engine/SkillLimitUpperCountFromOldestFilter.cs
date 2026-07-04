using System.Collections.Generic;
using System.Linq;

public class SkillLimitUpperCountFromOldestFilter : ISkillSelectFilter
{
	private int _limitCount;

	public SkillLimitUpperCountFromOldestFilter(int limitCount)
	{
		_limitCount = limitCount;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return -1;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		if (cards.Count() < _limitCount)
		{
			return cards;
		}
		return cards.Take(_limitCount);
	}
}
