using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetEnhanceCardfilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.EnhanceCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (option.EnhanceCard.IsPlayer == battlePlayerInfo.IsPlayer)
			{
				list.Add(option.EnhanceCard);
			}
		}
		return list;
	}
}
