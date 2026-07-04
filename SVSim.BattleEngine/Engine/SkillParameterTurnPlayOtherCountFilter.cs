using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnPlayOtherCountFilter : ISkillParameterSelectFilter
{
	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < cardInfos.Count(); i++)
		{
			BattleCardBase battleCardBase = cardInfos.ElementAt(i) as BattleCardBase;
			int num = battleCardBase.SelfBattlePlayer.GetCurrentTurnPlayCount();
			if (checkerOption != null && checkerOption.PlayedCard == battleCardBase)
			{
				num--;
			}
			list.Add(num);
		}
		return list;
	}
}
