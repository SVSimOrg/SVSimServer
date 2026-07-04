using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillPlayMomentSpellChargeFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo card) => card.SelfBattlePlayer.GamePlayMomentSpellChargeCards.Contains(card));
	}
}
