using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnHealValueFilter : ISkillParameterSelectFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnHealValueFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnHealValue(c, _turnPlayerInfo));
	}
}
