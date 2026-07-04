using System.Collections.Generic;
using Wizard.Battle;

public class SkillExclutionNoneFilter : ISkillExclutionFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue)
	{
		return cards;
	}
}
