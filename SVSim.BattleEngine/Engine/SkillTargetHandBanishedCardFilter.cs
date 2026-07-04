using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetHandBanishedCardFilter : ISkillTargetFilter
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
			if (battlePlayerInfo.IsPlayer == option.BanishedCard.IsPlayer && option.BanishedCard.BanishedInfo.Place == BattleCardBase.BanishInfo.BanishPlace.Hand)
			{
				list.Add(option.BanishedCard);
			}
		}
		return list;
	}
}
