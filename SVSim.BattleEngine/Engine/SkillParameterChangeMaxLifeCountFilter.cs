using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterChangeMaxLifeCountFilter : ISkillCardFilter
{
	private readonly int _parameter;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public SkillParameterChangeMaxLifeCountFilter(int parameter, string op)
	{
		_parameter = parameter;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.ChangeMaxLifeCount);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.SkillApplyInformation.GetChangeMaxLifeCount(), _parameter, c, cards));
	}
}
