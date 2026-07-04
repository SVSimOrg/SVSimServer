using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityEnhanceFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityEnhanceFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasEnhance;
		if (_parameterOptionText == "=")
		{
			hasEnhance = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasEnhance = false;
		}
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.HasSkillEnhance == hasEnhance)
			{
				yield return readOnlyBattleCardInfo;
			}
		}
	}
}
