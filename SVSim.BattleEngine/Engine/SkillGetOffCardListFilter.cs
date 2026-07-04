using System.Collections.Generic;
using Wizard.Battle;

public class SkillGetOffCardListFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			list.AddRange(card.GetOffCards);
		}
		return list;
	}
}
