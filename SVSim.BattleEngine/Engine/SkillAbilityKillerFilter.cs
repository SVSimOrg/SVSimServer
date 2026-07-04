using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityKillerFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityKillerFilter(string op)
	{
		_parameterOptionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasKiller;
		if (_parameterOptionText == "=")
		{
			hasKiller = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasKiller = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.SkillApplyInformation.IsKiller == hasKiller)
			{
				yield return card;
			}
		}
	}
}
