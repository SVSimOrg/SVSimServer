using System.Collections.Generic;
using Wizard;

public interface ISkillBattlePlayerFilter
{
	IEnumerable<IBattlePlayerReadOnlyInfo> Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair);
}
