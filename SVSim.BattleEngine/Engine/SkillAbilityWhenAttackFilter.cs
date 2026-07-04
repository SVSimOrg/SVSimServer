using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityWhenAttackFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasWhenAttack)
			{
				yield return card;
			}
		}
	}
}
