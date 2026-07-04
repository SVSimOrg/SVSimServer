using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSelectIdFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => (!c.BaseParameter.IsChoiceEvolutionCard) ? c.BaseParameter.NormalCardId : c.BaseParameter.BaseCardId);
	}
}
