using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterChargeCountFilter : ISkillCardFilter
{
	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public int Parameter { get; private set; }

	public string Option { get; private set; }

	public SkillParameterChargeCountFilter(int parameter, string op)
	{
		Parameter = parameter;
		Option = op;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.ChantCount);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.HasSpellCharge && _compareFunc(c.SpellChargeCount, Parameter, c, cards));
	}
}
