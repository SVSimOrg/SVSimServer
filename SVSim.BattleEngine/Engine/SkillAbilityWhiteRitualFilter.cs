using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityWhiteRitualFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL))
			{
				yield return card;
			}
		}
	}
}
