using System.Collections.Generic;
using Wizard;

public class SelfBattlePlayerFilter : ISkillBattlePlayerFilter
{
	public IEnumerable<IBattlePlayerReadOnlyInfo> Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		yield return playerInfoPair.ReadOnlySelf;
	}
}
