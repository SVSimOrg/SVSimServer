using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetBurialRiteCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.BurialRiteCards == null)
		{
			yield break;
		}
		foreach (IBattlePlayerReadOnlyInfo info in battlePlayerInfos)
		{
			foreach (BattleCardBase burialRiteCard in option.BurialRiteCards)
			{
				if (info.IsPlayer == burialRiteCard.IsPlayer)
				{
					yield return burialRiteCard;
				}
			}
		}
	}
}
