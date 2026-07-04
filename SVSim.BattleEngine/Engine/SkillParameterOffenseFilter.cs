using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterOffenseFilter : SkillCardFilterBase
{
	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterOffenseFilter(string parameterText, string op)
		: base(parameterText, op)
	{
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.Atk);
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		int value = option.ParseInt(_parameterText);
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.Atk, value, c, cards));
	}
}
