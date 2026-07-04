using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityQuickFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityQuickFilter(string op)
	{
		_parameterOptionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasQuick;
		if (_parameterOptionText == "=")
		{
			hasQuick = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasQuick = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.SkillApplyInformation.IsQuick == hasQuick)
			{
				yield return card;
			}
		}
	}
}
