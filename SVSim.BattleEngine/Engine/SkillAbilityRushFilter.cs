using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityRushFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityRushFilter(string op)
	{
		_parameterOptionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasRush;
		if (_parameterOptionText == "=")
		{
			hasRush = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasRush = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.SkillApplyInformation.IsRush == hasRush)
			{
				yield return card;
			}
		}
	}
}
