using System.Collections.Generic;
using Wizard.Battle;

public interface ISkillCustomSelectFilter
{
	IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option);
}
