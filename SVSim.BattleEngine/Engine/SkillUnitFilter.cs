using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillUnitFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where(IsUnit);
	}

	public static bool IsUnit(IReadOnlyBattleCardInfo card)
	{
		return card.IsUnit;
	}
}
