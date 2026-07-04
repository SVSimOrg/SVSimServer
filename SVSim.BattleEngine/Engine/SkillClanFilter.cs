using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillClanFilter : ISkillCardFilter
{
	public readonly CardBasePrm.ClanType _clan;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public readonly string OptionText;

	public SkillClanFilter(CardBasePrm.ClanType clan, string op)
	{
		_clan = clan;
		OptionText = op;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.Clan);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc((int)c.Clan, (int)_clan, c, cards));
	}
}
