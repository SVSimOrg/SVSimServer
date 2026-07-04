using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterChantCountFilter : SkillCardFilterBase
{
	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterChantCountFilter(string parameterText, string op)
		: base(parameterText, op)
	{
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.ChantCount);
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		int param = option.ParseInt(_parameterText);
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.ChantCount, param, c, cards));
	}
}
