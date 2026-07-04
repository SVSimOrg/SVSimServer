using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillLastBurialRiteCardFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < cards.Count(); i++)
		{
			BattleCardBase battleCardBase = cards.ElementAt(i) as BattleCardBase;
			list.AddRange(battleCardBase.SkillApplyInformation.LastBurialRiteCardList);
		}
		return list;
	}
}
