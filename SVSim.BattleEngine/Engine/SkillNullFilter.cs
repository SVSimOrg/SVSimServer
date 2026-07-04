using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillNullFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return Enumerable.Empty<IReadOnlyBattleCardInfo>();
	}
}
