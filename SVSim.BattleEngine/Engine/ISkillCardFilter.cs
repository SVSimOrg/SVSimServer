using System.Collections.Generic;
using Wizard.Battle;

public interface ISkillCardFilter
{
	IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option);
}
