using System.Collections.Generic;
using System.Linq;

public class SkillSelectIndexFilter : ISkillSelectFilter
{
	private readonly int _index;

	public SkillSelectIndexFilter(int index)
	{
		_index = index;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return 1;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		IEnumerable<BattleCardBase> result = new BattleCardBase[0];
		if (checkerOption.SelectedCards == null || checkerOption.SelectedCards.Count == 0)
		{
			return result;
		}
		BattleCardBase selectCard = checkerOption.SelectedCards[_index].SelectCard;
		if (cards.Contains(selectCard))
		{
			result = new BattleCardBase[1] { selectCard };
		}
		return result;
	}
}
