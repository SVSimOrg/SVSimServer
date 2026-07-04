using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.Card;

namespace Wizard.Battle.Operation;

public class SimulateCostNoDuplicationRandomSelectFilter : ISkillSelectFilter
{
	private readonly IVirtualBattleCard _virtualOwnerCard;

	private readonly string _contText;

	public SimulateCostNoDuplicationRandomSelectFilter(IVirtualBattleCard virtualOwnerCard, string randomCountText)
	{
		_virtualOwnerCard = virtualOwnerCard;
		_contText = randomCountText;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return option.ParseInt(_contText);
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		_virtualOwnerCard.UsedRandomSkill = true;
		int count = CalcCount(option);
		List<BattleCardBase> cardList = cards.ToList();
		for (int i = 0; i < count; i++)
		{
			if (cardList.Count != 0)
			{
				BattleCardBase card = cardList[0];
				cardList = cardList.Where((BattleCardBase c) => c.Card.BaseParameter.Cost != card.BaseParameter.Cost).ToList();
				yield return card;
				continue;
			}
			break;
		}
	}
}
