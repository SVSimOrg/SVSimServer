using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityGuardFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityGuardFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasGuard;
		if (_parameterOptionText == "=")
		{
			hasGuard = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasGuard = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.SkillApplyInformation.IsGuard == hasGuard)
			{
				yield return card;
			}
		}
	}
}
