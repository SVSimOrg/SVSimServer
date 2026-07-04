using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnHealCountTextFilter : SkillParameterTurnHealCountFilter
{
	public SkillParameterTurnHealCountTextFilter(string option)
		: base(option)
	{
	}

	public override IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.GetSpecificTurnHealCountOnlySelf(c, _turnPlayerInfo));
	}
}
