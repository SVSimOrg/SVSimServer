using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetFusionIngredientedCardListIncludeThisFusion : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetFusionIngredientedCardListIncludeThisFusion(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (option.FusionIngredientCards == null)
		{
			return list;
		}
		list.AddRange(_ownerCard.FusionIngredients);
		int i;
		for (i = 0; i < battlePlayerInfos.Count(); i++)
		{
			list.AddRange(option.FusionIngredientCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == battlePlayerInfos.ElementAt(i).IsPlayer));
		}
		return list;
	}
}
