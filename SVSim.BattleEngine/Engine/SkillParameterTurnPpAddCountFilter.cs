using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnPpAddCountFilter : ISkillParameterSelectFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnPpAddCountFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnPpAddCount(c, _turnPlayerInfo));
	}
}
