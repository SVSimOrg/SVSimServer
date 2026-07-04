using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetGameSummonCardsFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(battlePlayerInfo.SkillInfoGameSummonCards.Select((BattlePlayerBase.TurnAndCard c) => c.Card));
		}
		return list;
	}
}
