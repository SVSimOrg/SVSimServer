using System.Collections.Generic;
using Wizard;

public class BothBattlePlayerFilter : ISkillBattlePlayerFilter
{
	public IEnumerable<IBattlePlayerReadOnlyInfo> Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		yield return playerInfoPair.ReadOnlySelf;
		yield return playerInfoPair.ReadOnlyOpponent;
	}
}
