using System.Collections.Generic;
using Wizard.Battle;

public interface ISkillParameterSelectFilter
{
	IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption = null);
}
