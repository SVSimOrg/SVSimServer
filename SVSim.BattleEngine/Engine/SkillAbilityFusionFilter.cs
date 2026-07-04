using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityFusionFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityFusionFilter(string optionText)
	{
		_parameterOptionText = optionText;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool flag = _parameterOptionText == "=";
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.HasSkillFusion == flag)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
