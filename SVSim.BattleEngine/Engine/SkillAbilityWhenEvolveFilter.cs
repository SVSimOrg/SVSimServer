using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityWhenEvolveFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityWhenEvolveFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool flag = _parameterOptionText == "=";
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			if (cards.ElementAt(i).HasSkillWhenEvolve == flag)
			{
				list.Add(cards.ElementAt(i));
			}
		}
		return list;
	}
}
