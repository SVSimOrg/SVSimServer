using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetSkillDrawCardListFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			list.AddRange(cards.ElementAt(i).SkillApplyInformation.SkillDrewCardList);
		}
		return list;
	}
}
