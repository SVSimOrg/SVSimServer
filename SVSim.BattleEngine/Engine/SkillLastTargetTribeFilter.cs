using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillLastTargetTribeFilter : ISkillCardFilter
{
	private readonly bool _isEqual;

	public SkillLastTargetTribeFilter(string op)
	{
		_isEqual = op == "=";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		for (int i = 0; i < cards.Count(); i++)
		{
			if (!(cards.ElementAt(i) is BattleCardBase card))
			{
				continue;
			}
			List<BattleCardBase> lastTarget = card.SelfBattlePlayer.LastTargetCardsList.FirstOrDefault();
			for (int j = 0; j < lastTarget.Count; j++)
			{
				if (card.Tribe.FindAll(lastTarget[j].Tribe.Contains).Count > 0 == _isEqual)
				{
					yield return card;
				}
			}
		}
	}
}
