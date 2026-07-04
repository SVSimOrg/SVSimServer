using System.Collections.Generic;

public class SkillUserSelectFilter : ISkillSelectFilter
{
	public bool IsEmptyHanded;

	private readonly string EMPTY_HANDED = "empty_handed";

	private string _context = "";

	public SkillUserSelectFilter(string value)
	{
		IsEmptyHanded = value == EMPTY_HANDED;
		_context = value;
		if (IsEmptyHanded)
		{
			_context = "1";
		}
	}

	public int CalcCount(SkillOptionValue option)
	{
		return option.ParseInt(_context);
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		return cards;
	}
}
