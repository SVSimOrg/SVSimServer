using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetInHandCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.InHandCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo info in battlePlayerInfos)
		{
			list.AddRange(option.InHandCard.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == info.IsPlayer));
		}
		return list;
	}
}
