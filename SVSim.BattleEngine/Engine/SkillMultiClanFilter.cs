using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillMultiClanFilter : ISkillCardFilter
{
	public readonly List<CardBasePrm.ClanType> ClanList;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public readonly string OptionText;

	public SkillMultiClanFilter(List<CardBasePrm.ClanType> clanList, string op)
	{
		ClanList = clanList;
		OptionText = op;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.Clan);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		int i;
		for (i = 0; i < ClanList.Count; i++)
		{
			list.AddRange(cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc((int)c.Clan, (int)ClanList[i], c, cards)));
		}
		return list;
	}
}
