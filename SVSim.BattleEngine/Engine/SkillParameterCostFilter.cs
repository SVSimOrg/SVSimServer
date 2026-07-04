using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterCostFilter : SkillCardFilterBase
{
	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterCostFilter(string parameterText, string op)
		: base(parameterText, op)
	{
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.Cost);
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		int value = option.ParseInt(_parameterText);
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.Cost, value, c, cards));
	}
}
