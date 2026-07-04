using System.Collections.Generic;
using Wizard.Battle;

public class SkillParameterTurnSummonCountFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		IReadOnlyBattleCardInfo summonedCard = checkerOption.SummonedCard;
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>(cardInfos);
		return new List<int> { list.LastIndexOf(summonedCard) + 1 };
	}
}
