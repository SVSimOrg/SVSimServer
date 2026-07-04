using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterMaxLifeFilter : ISkillCardFilter
{
	private readonly string _parameterText;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterMaxLifeFilter(string parameterText, string op)
	{
		_parameterText = parameterText;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.MaxLife);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		int value = option.ParseInt(_parameterText);
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.MaxLife, value, c, cards));
	}
}
