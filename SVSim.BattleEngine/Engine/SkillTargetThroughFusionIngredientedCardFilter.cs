using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetThroughFusionIngredientedCardFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	private int _index;

	public SkillTargetThroughFusionIngredientedCardFilter(IReadOnlyBattleCardInfo ownerCard, string index)
	{
		_ownerCard = ownerCard;
		_index = int.Parse(index);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.FusionIngredientCards == null)
		{
			return list;
		}
		List<IReadOnlyBattleCardInfo> list2 = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo info in battlePlayerInfos)
		{
			list2.AddRange(option.FusionIngredientCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == info.IsPlayer));
		}
		if (_ownerCard.FusionIngredients.Count >= _index && _ownerCard.FusionIngredients.Count - list2.Count < _index)
		{
			list.Add(_ownerCard.FusionIngredients[_index - 1]);
		}
		return list;
	}
}
