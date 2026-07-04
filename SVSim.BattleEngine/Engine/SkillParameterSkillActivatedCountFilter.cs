using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSkillActivatedCountFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillActivatedCount);
	}
}
