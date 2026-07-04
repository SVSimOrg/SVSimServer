using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetBanishedCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.BanishedCard == null)
		{
			yield break;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (battlePlayerInfo.IsPlayer == option.BanishedCard.IsPlayer)
			{
				yield return option.BanishedCard;
			}
		}
	}
}
