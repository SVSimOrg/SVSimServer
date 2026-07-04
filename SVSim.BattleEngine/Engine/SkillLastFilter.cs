using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillLastFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (cards.Count() > 0)
		{
			list.Add(cards.Last());
		}
		return list;
	}
}
