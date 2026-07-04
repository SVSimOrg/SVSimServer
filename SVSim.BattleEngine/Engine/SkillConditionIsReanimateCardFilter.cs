using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillConditionIsReanimateCardFilter : ISkillCardFilter
{
	protected readonly bool _isReanimate;

	public SkillConditionIsReanimateCardFilter(bool isReanimate)
	{
		_isReanimate = isReanimate;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.IsReanimate == _isReanimate);
	}
}
