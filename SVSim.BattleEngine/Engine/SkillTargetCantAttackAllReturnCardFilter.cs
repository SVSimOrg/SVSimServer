using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetCantAttackAllReturnCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.CantAttackAllReturnCards == null)
		{
			return list;
		}
		int i;
		for (i = 0; i < battlePlayerInfos.Count(); i++)
		{
			list.AddRange(option.CantAttackAllReturnCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == battlePlayerInfos.ElementAt(i).IsPlayer));
		}
		return list;
	}
}
