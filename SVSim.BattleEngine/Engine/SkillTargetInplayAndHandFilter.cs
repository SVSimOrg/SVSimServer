using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetInplayAndHandFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < battlePlayerInfos.Count(); i++)
		{
			IBattlePlayerReadOnlyInfo battlePlayerReadOnlyInfo = battlePlayerInfos.ElementAt(i);
			list.AddRange(battlePlayerReadOnlyInfo.SkillInfoClassAndInPlayCards);
			list.AddRange(battlePlayerReadOnlyInfo.SkillInfoHandCards);
		}
		return list;
	}
}
