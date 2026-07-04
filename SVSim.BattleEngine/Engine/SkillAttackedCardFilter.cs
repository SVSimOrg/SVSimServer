using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAttackedCardFilter : ISkillCardFilter
{
	private bool isAttacked;

	public SkillAttackedCardFilter(string value)
	{
		isAttacked = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c is BattleCardBase && isAttacked == ((c as BattleCardBase).AttackableCount != (c as BattleCardBase).MaxAttackableCount));
	}
}
