using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetSkillSummonedCardIdFilter : ISkillCardFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			list.AddRange(card.SkillApplyInformation.RandomSelectedCardList);
		}
		return list;
	}
}
