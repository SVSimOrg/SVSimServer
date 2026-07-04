using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterMaxAttackCountFilter : ISkillCardFilter
{
	private readonly int parameter;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterMaxAttackCountFilter(string value, string op)
	{
		parameter = int.Parse(value);
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.MaxAttackableCount);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.MaxAttackableCount, parameter, c, cards));
	}
}
