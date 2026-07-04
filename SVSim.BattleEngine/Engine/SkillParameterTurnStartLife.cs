using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnStartLife : ISkillParameterSelectFilter
{
	protected readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnStartLife(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < cardInfos.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cardInfos.ElementAt(i);
			IEnumerable<int> collection = from t in readOnlyBattleCardInfo.SkillApplyInformation.GetSpecificTurnStartLifeList(readOnlyBattleCardInfo, _turnPlayerInfo)
				select t.Value;
			list.AddRange(collection);
		}
		return list;
	}
}
