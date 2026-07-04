using System.Collections.Generic;
using Wizard.Battle;

public interface ISkillTargetFilter
{
	IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option);
}
