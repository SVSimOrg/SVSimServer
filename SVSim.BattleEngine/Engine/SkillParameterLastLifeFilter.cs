using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterLastLifeFilter : ISkillCardFilter
{
	private readonly int parameter;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	private string _optionText = "";

	public SkillParameterLastLifeFilter(int parameter, string op)
	{
		this.parameter = parameter;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.LastLife);
		_optionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.SkillApplyInformation.GetLastLife(), parameter, c, cards));
	}
}
