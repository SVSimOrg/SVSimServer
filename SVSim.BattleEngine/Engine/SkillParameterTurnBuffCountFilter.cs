using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnBuffCountFilter : ISkillParameterSelectFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnBuffCountFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnBuffCount(_turnPlayerInfo));
	}
}
