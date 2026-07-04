using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilitySneakFilter : ISkillCardFilter
{

	private readonly string _parameterOptionText;

	public SkillAbilitySneakFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasSneak;
		if (_parameterOptionText == "=")
		{
			hasSneak = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasSneak = false;
		}
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.SkillApplyInformation.IsSneak == hasSneak)
			{
				yield return card;
			}
		}
	}
}
