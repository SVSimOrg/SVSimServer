using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterTurnFusionCountTextFilter : ISkillParameterSelectFilter
{
	protected readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillParameterTurnFusionCountTextFilter(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption = null)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => (c.IsPlayer == c.SelfBattlePlayer.BattleMgr.BattlePlayer.IsSelfTurn) ? c.SkillApplyInformation.GetSpecificTurnFusionCount(c, _turnPlayerInfo) : 0);
	}
}
