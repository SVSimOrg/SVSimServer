using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityAccelerateFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityAccelerateFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasAccelerate;
		switch (_parameterOptionText)
		{
		default:
			yield break;
		case "=":
			hasAccelerate = true;
			break;
		case "!=":
			hasAccelerate = false;
			break;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasSkillAccelerate == hasAccelerate)
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
