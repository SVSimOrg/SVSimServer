using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityDrainFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityDrainFilter(string op)
	{
		_parameterOptionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasDrain;
		if (_parameterOptionText == "=")
		{
			hasDrain = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasDrain = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.SkillApplyInformation.IsDrain == hasDrain)
			{
				yield return card;
			}
		}
	}
}
