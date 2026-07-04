using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetSummonedCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		BattleCardBase summonedCard = option.SummonedCard;
		if (option.SummonedCard != null && battlePlayerInfos.Any((IBattlePlayerReadOnlyInfo c) => c.IsPlayer == summonedCard.IsPlayer))
		{
			yield return option.SummonedCard;
		}
	}
}
