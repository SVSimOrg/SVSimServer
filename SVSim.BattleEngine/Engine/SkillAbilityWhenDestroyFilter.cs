using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityWhenDestroyFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityWhenDestroyFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasWhenDestroy = IsOperaterEqual();
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasSkillWhenDestroy == hasWhenDestroy)
			{
				yield return card;
			}
		}
	}

	public bool IsOperaterEqual()
	{
		return _parameterOptionText == "=";
	}
}
