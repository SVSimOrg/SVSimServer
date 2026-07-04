using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetDestroyedCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.DestroyedCard == null)
		{
			yield break;
		}
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (battlePlayerInfo.IsPlayer == option.DestroyedCard.IsPlayer)
			{
				yield return option.DestroyedCard;
			}
		}
	}
}
