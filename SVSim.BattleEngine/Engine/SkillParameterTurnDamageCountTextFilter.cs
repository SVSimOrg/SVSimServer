using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnDamageCountTextFilter : SkillParameterTurnDamageCountFilter
{
	public SkillParameterTurnDamageCountTextFilter(string option)
		: base(option)
	{
	}

	public override IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption = null)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => (c.IsPlayer == c.SelfBattlePlayer.BattleMgr.BattlePlayer.IsSelfTurn) ? c.SkillApplyInformation.GetSpecificTurnDamageCount(c, _turnPlayerInfo) : 0);
	}
}
