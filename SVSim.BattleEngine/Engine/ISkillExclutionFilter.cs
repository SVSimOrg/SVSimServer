using System.Collections.Generic;
using Wizard.Battle;

public interface ISkillExclutionFilter
{
	IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue);
}
