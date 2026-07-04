using System.Collections.Generic;
using System.Linq;

public class SkillLimitUpperCountFromNewestFilter : ISkillSelectFilter
{
	private int _limitCount;

	public SkillLimitUpperCountFromNewestFilter(int limitCount)
	{
		_limitCount = limitCount;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return -1;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		IEnumerable<BattleCardBase> enumerable = cards.Except(checkerOption.SkillDrewCards.Cast<BattleCardBase>());
		if (enumerable.Count() < _limitCount)
		{
			return enumerable;
		}
		return enumerable.Skip(enumerable.Count() - _limitCount);
	}
}
