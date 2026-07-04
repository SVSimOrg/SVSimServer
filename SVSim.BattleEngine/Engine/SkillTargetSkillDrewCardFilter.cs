using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetSkillDrewCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.SkillDrewCards == null)
		{
			return new IReadOnlyBattleCardInfo[0];
		}
		return option.SkillDrewCards;
	}
}
