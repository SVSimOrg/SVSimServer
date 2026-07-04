using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.Card;

namespace Wizard.Battle.Operation;

public class SimulateRandomSelectFilter : ISkillSelectFilter
{
	private readonly IVirtualBattleCard _virtualOwnerCard;

	private readonly string _contText;

	public SimulationSelection _selection;

	public SimulateRandomSelectFilter(IVirtualBattleCard virtualOwnerCard, string randomCountText)
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
		if (_selection == SimulationSelection.None)
		{
			return new List<BattleCardBase>();
		}
		if (_selection == SimulationSelection.All)
		{
			return cards;
		}
		return cards.Take(CalcCount(option));
	}
}
