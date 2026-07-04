using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillUniqueBaseCostCountFilter : SkillCardCountFilter
{
	public override int Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (!list.Contains(readOnlyBattleCardInfo.BaseCost))
			{
				list.Add(readOnlyBattleCardInfo.BaseCost);
			}
		}
		return list.Count;
	}
}
