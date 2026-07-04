using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnHealCountFilter : ISkillParameterSelectFilter
{
	protected readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnHealCountFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnHealCount(c, _turnPlayerInfo));
	}
}
