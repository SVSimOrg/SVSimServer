using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SpellSelectableCardFilter : SkillSelectableCardFilter
{
	public SpellSelectableCardFilter(IReadOnlyBattleCardInfo card)
		: base(card)
	{
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => _card.IsPlayer == c.IsPlayer || (!c.SkillApplyInformation.CantBeFocusedSkill && !c.SkillApplyInformation.CantBeFocusedSpell));
	}
}
