using System.Collections.Generic;

public class SkillChoiceSelectFilter : ISkillSelectFilter
{
	private readonly string _context;

	private int _count = 1;

	public int Count => _count;

	public SkillChoiceSelectFilter(string countText)
	{
		_context = countText;
		bool flag = int.TryParse(_context, out _count);
		_count = ((!flag) ? 1 : _count);
	}

	public int CalcCount(SkillOptionValue option)
	{
		return option.ParseInt(_context);
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		_count = CalcCount(option);
		return cards;
	}
}
