using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetInplayBanishedCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.BanishedCard == null)
		{
			return list;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (battlePlayerInfo.IsPlayer == option.BanishedCard.IsPlayer && option.BanishedCard.BanishedInfo.Place == BattleCardBase.BanishInfo.BanishPlace.Field)
			{
				list.Add(option.BanishedCard);
			}
		}
		return list;
	}
}
