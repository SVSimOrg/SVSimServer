using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnAcceleratedCardCountTextFilter : SkillParameterTurnAcceleratedCardCountFilter
{
	public SkillParameterTurnAcceleratedCardCountTextFilter(string option)
		: base(option)
	{
	}

	public override IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnAcceleratedCardCountOnlySelf(c, _turnPlayerInfo));
	}
}
