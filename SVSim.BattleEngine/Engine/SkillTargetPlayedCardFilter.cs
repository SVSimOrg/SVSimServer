using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetPlayedCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.PlayedCard == null)
		{
			yield break;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (option.PlayedCard.IsPlayer == battlePlayerInfo.IsPlayer)
			{
				yield return option.PlayedCard;
			}
		}
	}
}
