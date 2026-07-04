using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetCrystallizedCardfilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.CrystallizedCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (battlePlayerInfo.IsPlayer == option.CrystallizedCard.IsPlayer)
			{
				list.Add(option.CrystallizedCard);
			}
		}
		return list;
	}
}
