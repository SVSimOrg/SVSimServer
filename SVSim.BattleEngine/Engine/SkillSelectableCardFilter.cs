using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillSelectableCardFilter : ISkillCardFilter
{
	protected IReadOnlyBattleCardInfo _card;

	public SkillSelectableCardFilter(IReadOnlyBattleCardInfo card)
	{
		_card = card;
	}

	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => _card.IsPlayer == c.IsPlayer || !c.SkillApplyInformation.CantBeFocusedSkill);
	}
}
