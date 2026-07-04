using System.Collections.Generic;
using System.Linq;

public class SkillOnlyOneFilter : ISkillSelectFilter
{
	private readonly string _parameterText;

	public SkillOnlyOneFilter(string parameterText)
	{
		_parameterText = parameterText;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return 1;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		int num = option.ParseInt(_parameterText);
		List<BattleCardBase> list = cards.ToList();
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		if (num < 0 || list.Count <= num)
		{
			return list2;
		}
		if (list[num] != null)
		{
			list2.Add(list[num]);
		}
		return list2;
	}
}
