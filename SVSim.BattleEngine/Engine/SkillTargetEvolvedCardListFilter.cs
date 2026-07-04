using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetEvolvedCardListFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoEvolvedCards);
	}
}
