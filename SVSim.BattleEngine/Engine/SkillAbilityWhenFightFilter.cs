using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityWhenFightFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasWhenFight)
			{
				yield return card;
			}
		}
	}
}
