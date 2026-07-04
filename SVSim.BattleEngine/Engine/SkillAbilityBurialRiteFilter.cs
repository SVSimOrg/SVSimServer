using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityBurialRiteFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityBurialRiteFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool flag = _parameterOptionText == "=";
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			if (cards.ElementAt(i).HasSkillBurialRite == flag)
			{
				list.Add(cards.ElementAt(i));
			}
		}
		return list;
	}
}
