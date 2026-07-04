using System.Collections.Generic;
using Wizard.Battle;

public class SkillAllCardFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards;
	}
}
