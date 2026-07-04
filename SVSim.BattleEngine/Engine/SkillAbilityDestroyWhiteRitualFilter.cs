using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityDestroyWhiteRitualFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasSkillDestroyWhiteRitual)
			{
				yield return card;
			}
		}
	}
}
