using System.Collections.Generic;

public class SkillSelectAllFilter : ISkillSelectFilter
{
	public int CalcCount(SkillOptionValue option)
	{
		return -1;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		return cards;
	}
}
