using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDeckDrawCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.DeckDrewCards == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo info in battlePlayerInfos)
		{
			list.AddRange(option.DeckDrewCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == info.IsPlayer));
		}
		return list;
	}
}
