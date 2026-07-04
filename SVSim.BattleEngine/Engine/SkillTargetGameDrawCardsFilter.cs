using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetGameDrawCardsFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(battlePlayerInfo.SkillInfoGameDrawCards);
			list.AddRange(battlePlayerInfo.SkillInfoGameDrawTokenCards);
		}
		return list;
	}
}
