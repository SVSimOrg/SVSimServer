using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillFusionIngredientCardListFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> list2 = cards.ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			list.AddRange(list2[i].FusionIngredients);
		}
		return list;
	}
}
