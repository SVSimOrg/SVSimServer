using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityCrystallizeFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityCrystallizeFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasCrystallize;
		switch (_parameterOptionText)
		{
		default:
			yield break;
		case "=":
			hasCrystallize = true;
			break;
		case "!=":
			hasCrystallize = false;
			break;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasSkillCrystallize == hasCrystallize)
			{
				yield return card;
			}
		}
	}

	public bool IsOperaterEqual()
	{
		return _parameterOptionText switch
		{
			"=" => true, 
			"!=" => false, 
			_ => true, 
		};
	}
}
