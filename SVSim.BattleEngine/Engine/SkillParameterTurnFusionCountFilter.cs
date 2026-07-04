using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnFusionCountFilter : ISkillParameterSelectFilter
{
	protected readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnFusionCountFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption = null)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnFusionCount(c, _turnPlayerInfo));
	}
}
