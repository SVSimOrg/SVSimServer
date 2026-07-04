using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetNullFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return Enumerable.Empty<IReadOnlyBattleCardInfo>();
	}
}
