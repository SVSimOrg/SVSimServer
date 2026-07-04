using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillSummonMomentTribeFilter : ISkillCardFilter
{
	private readonly CardBasePrm.TribeType _type;

	private readonly bool _isEqual;

	public SkillSummonMomentTribeFilter(CardBasePrm.TribeType tribe, string op)
	{
		_type = tribe;
		_isEqual = op == "=";
	}

	private bool judgeSummonMomentTribe(IReadOnlyBattleCardInfo readOnlyBattleCard, CardBasePrm.TribeType tribe)
	{
		BattleCardBase card = readOnlyBattleCard as BattleCardBase;
		List<List<CardBasePrm.TribeType>> list = (from c in card.SelfBattlePlayer.GameSummonMomentTribe
			where c.Card == card
			select c.Tribes).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			if (list[num].Contains(tribe))
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => judgeSummonMomentTribe(c, _type) == _isEqual);
	}
}
