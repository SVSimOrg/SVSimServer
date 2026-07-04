using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSelectUnionBurstCountFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.UnionBurstCount);
	}
}
