using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterDestroyedByCardIdFilter : ISkillCardFilter
{
	private readonly string _parameterText;

	public SkillParameterDestroyedByCardIdFilter(string parameterText)
	{
		_parameterText = parameterText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		int id = option.ParseInt(_parameterText);
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo card = cards.ElementAt(i);
			for (int j = 0; j < card.DestroyedBySkillList.Count; j++)
			{
				if (card.DestroyedBySkillList.ElementAt(j).BaseCardId == id)
				{
					yield return card;
				}
			}
		}
	}
}
