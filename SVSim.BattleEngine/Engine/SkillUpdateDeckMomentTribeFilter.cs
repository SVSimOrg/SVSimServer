using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillUpdateDeckMomentTribeFilter : ISkillCardFilter
{
	private readonly CardBasePrm.TribeType _type;

	private readonly bool _isEqual;

	public SkillUpdateDeckMomentTribeFilter(CardBasePrm.TribeType tribe, string op)
	{
		_type = tribe;
		_isEqual = op == "=";
	}

	private bool JudgeUpdateDeckMomentTribe(IReadOnlyBattleCardInfo readOnlyBattleCard, CardBasePrm.TribeType tribe)
	{
		BattleCardBase card = readOnlyBattleCard as BattleCardBase;
		List<List<CardBasePrm.TribeType>> list = (from c in card.SelfBattlePlayer.GameUpdateDeckMomentTribe
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
		return cards.Where((IReadOnlyBattleCardInfo c) => JudgeUpdateDeckMomentTribe(c, _type) == _isEqual);
	}
}
