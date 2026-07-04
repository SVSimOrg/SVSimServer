using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDrewOverHandLimitFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.DrewOverHandLimitCards == null)
		{
			return list;
		}
		int i;
		for (i = 0; i < battlePlayerInfos.Count(); i++)
		{
			list.AddRange(option.DrewOverHandLimitCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == battlePlayerInfos.ElementAt(i).IsPlayer));
		}
		return list;
	}
}
