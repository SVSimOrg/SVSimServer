using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterHasSkillFilter : ISkillCardFilter
{
	private bool _hasSkill;

	public SkillParameterHasSkillFilter(string value)
	{
		_hasSkill = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.HasAnySkill == _hasSkill);
	}
}
