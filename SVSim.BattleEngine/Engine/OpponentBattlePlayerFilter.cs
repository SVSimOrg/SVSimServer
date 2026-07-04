using System.Collections.Generic;
using Wizard;

public class OpponentBattlePlayerFilter : ISkillBattlePlayerFilter
{
	public IEnumerable<IBattlePlayerReadOnlyInfo> Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		yield return playerInfoPair.ReadOnlyOpponent;
	}
}
