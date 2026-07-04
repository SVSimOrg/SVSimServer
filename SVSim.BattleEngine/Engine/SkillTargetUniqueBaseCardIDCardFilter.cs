using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetUniqueBaseCardIDCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards.Distinct(new BaseCardIDComp()));
	}
}
