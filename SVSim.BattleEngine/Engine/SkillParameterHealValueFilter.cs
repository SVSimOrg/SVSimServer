using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterHealValueFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		if (checkerOption.HealingCardAndValue == null)
		{
			return new List<int>();
		}
		return from h in checkerOption.HealingCardAndValue
			where cardInfos.Contains(h.Card)
			select h.Value;
	}
}
