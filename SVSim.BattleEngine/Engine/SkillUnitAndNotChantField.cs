using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillUnitAndNotChantField : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.IsUnit || (c.IsField && !c.IsChantField));
	}
}
