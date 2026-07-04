using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillConditionIsInplayCardFilter : ISkillCardFilter
{
	protected readonly bool _isInplay;

	public SkillConditionIsInplayCardFilter(bool isInplay)
	{
		_isInplay = isInplay;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.IsInplay == _isInplay);
	}
}
