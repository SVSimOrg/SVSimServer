using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetAttackerFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.AttackerCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (option.AttackerCard.IsPlayer == battlePlayerInfo.IsPlayer)
			{
				list.Add(option.AttackerCard);
			}
		}
		return list;
	}
}
