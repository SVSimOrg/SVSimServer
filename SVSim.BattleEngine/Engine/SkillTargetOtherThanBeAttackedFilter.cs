using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetOtherThanBeAttackedFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			foreach (IReadOnlyBattleCardInfo skillInfoInPlayCard in battlePlayerInfo.SkillInfoInPlayCards)
			{
				if (skillInfoInPlayCard != option.AttackTargetCard)
				{
					list.Add(skillInfoInPlayCard);
				}
			}
		}
		return list;
	}
}
