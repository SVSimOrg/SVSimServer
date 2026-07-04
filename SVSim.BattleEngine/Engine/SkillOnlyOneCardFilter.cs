using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillOnlyOneCardFilter : ISkillCardFilter
{
	private int _index;

	public SkillOnlyOneCardFilter(int index)
	{
		_index = index;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = cards.ToList();
		List<IReadOnlyBattleCardInfo> list2 = new List<IReadOnlyBattleCardInfo>();
		if (_index < 0 || list.Count <= _index)
		{
			return list2;
		}
		if (list[_index] != null)
		{
			list2.Add(list[_index]);
		}
		return list2;
	}
}
