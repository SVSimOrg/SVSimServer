using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterPreviousTurnAttackedFilter : ISkillCardFilter
{
	private bool isAttacked;

	public SkillParameterPreviousTurnAttackedFilter(string value)
	{
		isAttacked = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c is BattleCardBase && (c as BattleCardBase).IsPreviousTurnAttacked == isAttacked);
	}
}
