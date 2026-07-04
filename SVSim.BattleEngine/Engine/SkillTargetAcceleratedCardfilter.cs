using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetAcceleratedCardfilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.AcceleratedCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (battlePlayerInfo.IsPlayer == option.AcceleratedCard.IsPlayer)
			{
				list.Add(option.AcceleratedCard);
			}
		}
		return list;
	}
}
