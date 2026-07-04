using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterDestroyFilter : ISkillCardFilter
{
	private bool isDestroy;

	public SkillParameterDestroyFilter(string value)
	{
		isDestroy = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.IsDead == isDestroy);
	}
}
