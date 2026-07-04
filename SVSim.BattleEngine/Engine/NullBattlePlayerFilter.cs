using System.Collections.Generic;
using System.Linq;
using Wizard;

public class NullBattlePlayerFilter : ISkillBattlePlayerFilter
{
	public IEnumerable<IBattlePlayerReadOnlyInfo> Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		return Enumerable.Empty<IBattlePlayerReadOnlyInfo>();
	}
}
