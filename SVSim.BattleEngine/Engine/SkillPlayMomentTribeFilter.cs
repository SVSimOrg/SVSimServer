using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillPlayMomentTribeFilter : ISkillCardFilter
{
	private readonly CardBasePrm.TribeType _type;

	private readonly bool _isEqual;

	public SkillPlayMomentTribeFilter(CardBasePrm.TribeType tribe, string op)
	{
		_type = tribe;
		_isEqual = op == "=";
	}

	private bool judgePlayMomentTribe(IReadOnlyBattleCardInfo readOnlyBattleCard, CardBasePrm.TribeType tribe)
	{
		BattleCardBase card = readOnlyBattleCard as BattleCardBase;
		List<List<CardBasePrm.TribeType>> list = (from c in card.SelfBattlePlayer.GamePlayMomentTribe
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
		return cards.Where((IReadOnlyBattleCardInfo c) => judgePlayMomentTribe(c, _type) == _isEqual);
	}
}
