using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnAcceleratedCardCountFilter : ISkillParameterSelectFilter
{
	protected readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnAcceleratedCardCountFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnAcceleratedCardCount(c, _turnPlayerInfo));
	}
}
