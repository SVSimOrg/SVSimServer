using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetBeAttackedFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.AttackTargetCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (option.AttackTargetCard.IsPlayer == battlePlayerInfo.IsPlayer)
			{
				list.Add(option.AttackTargetCard);
			}
		}
		return list;
	}
}
