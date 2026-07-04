using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetFusionThisTurnCardList : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoTurnFusionCards);
	}
}
