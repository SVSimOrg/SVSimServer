using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetSkillUpdateDeckCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.SkillUpdatedDeckCards == null)
		{
			return new IReadOnlyBattleCardInfo[0];
		}
		return option.SkillUpdatedDeckCards;
	}
}
