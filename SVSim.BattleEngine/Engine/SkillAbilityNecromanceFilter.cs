using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityNecromanceFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityNecromanceFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool flag = _parameterOptionText == "=";
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			if (cards.ElementAt(i) is BattleCardBase battleCardBase && battleCardBase.HasSkillNecromance == flag)
			{
				list.Add(battleCardBase);
			}
		}
		return list;
	}
}
