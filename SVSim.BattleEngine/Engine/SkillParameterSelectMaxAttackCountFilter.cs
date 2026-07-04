using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSelectMaxAttackCountFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.MaxAttackableCount);
	}
}
