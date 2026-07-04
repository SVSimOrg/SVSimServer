using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilitySpellChargeFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilitySpellChargeFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasSpellCharge;
		if (_parameterOptionText == "=")
		{
			hasSpellCharge = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasSpellCharge = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasSpellCharge == hasSpellCharge)
			{
				yield return card;
			}
		}
	}

	public string GetOptionParameter()
	{
		return _parameterOptionText;
	}
}
