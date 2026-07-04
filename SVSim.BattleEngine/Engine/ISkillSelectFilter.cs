using System.Collections.Generic;

public interface ISkillSelectFilter
{
	int CalcCount(SkillOptionValue option);

	IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption);
}
