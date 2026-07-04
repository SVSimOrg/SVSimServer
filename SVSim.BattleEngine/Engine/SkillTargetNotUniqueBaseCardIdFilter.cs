using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetNotUniqueBaseCardIdFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (cards.Where((IReadOnlyBattleCardInfo c) => c.BaseParameter.BaseCardId == card.BaseParameter.BaseCardId).Count() >= 2)
			{
				list.Add(card);
			}
		}
		return list;
	}
}
