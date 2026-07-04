using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillBothClanFilter : ISkillCardFilter
{
	private readonly CardBasePrm.ClanType _clan;

	private readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	public readonly string OptionText;

	public SkillBothClanFilter(CardBasePrm.ClanType clan, string op)
	{
		_clan = clan;
		OptionText = op;
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.Clan);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		cards = cards.Where((IReadOnlyBattleCardInfo s) => s.IsClass);
		IEnumerable<IReadOnlyBattleCardInfo> enumerable = cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc((int)c.Clan, (int)_clan, c, cards));
		if (enumerable.Count() > 0)
		{
			return enumerable;
		}
		return cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.IsPlayer ? c.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().GetPlayerSubClassId() : c.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().GetEnemySubClassId(), (int)_clan, c, cards));
	}
}
