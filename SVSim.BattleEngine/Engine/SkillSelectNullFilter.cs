using System.Collections.Generic;
using System.Linq;

public class SkillSelectNullFilter : ISkillSelectFilter
{
	public int CalcCount(SkillOptionValue option)
	{
		return -1;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		return Enumerable.Empty<BattleCardBase>();
	}
}
