using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetGameFusionIngredientedCards : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(battlePlayerInfo.SkillInfoFusionIngredientList);
		}
		return list;
	}
}
