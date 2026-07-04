using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterAttackCountFilter : ISkillCardFilter
{
	private readonly int parameter;

	private readonly bool IsPreAction;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterAttackCountFilter(string value, string op)
	{
		if (value == "pre_action")
		{
			IsPreAction = true;
			return;
		}
		parameter = int.Parse(value);
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.AttackableCount);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		if (!IsPreAction)
		{
			return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.AttackableCount, parameter, c, cards));
		}
		return cards.Where((IReadOnlyBattleCardInfo c) => c.AttackableCount < c.MaxAttackableCount);
	}
}
