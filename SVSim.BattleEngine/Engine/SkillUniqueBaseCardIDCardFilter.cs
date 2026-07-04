using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillUniqueBaseCardIDCardFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Distinct(new BaseCardIDComp());
	}
}
