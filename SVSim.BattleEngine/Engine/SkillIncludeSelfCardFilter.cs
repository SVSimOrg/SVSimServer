using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillIncludeSelfCardFilter : ISkillCardFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	private bool _includeSelf;

	public SkillIncludeSelfCardFilter(IReadOnlyBattleCardInfo card, string value)
	{
		_ownerCard = card;
		_includeSelf = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo s) => (!_includeSelf) ? (s != _ownerCard) : (s == _ownerCard));
	}
}
