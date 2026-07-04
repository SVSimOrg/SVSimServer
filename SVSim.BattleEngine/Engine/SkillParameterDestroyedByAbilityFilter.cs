using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterDestroyedByAbilityFilter : ISkillCardFilter
{
	private readonly BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility _ability;

	public SkillParameterDestroyedByAbilityFilter(SkillFilterCreator.ContentKeyword keyword)
	{
		switch (keyword)
		{
		case SkillFilterCreator.ContentKeyword.when_play:
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.WhenPlay;
			break;
		case SkillFilterCreator.ContentKeyword.when_accelerate:
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.Accelerate;
			break;
		default:
			_ability = BattleCardBase.DestroyedBySkillInfo.DestroyedBySkillAbility.None;
			break;
		}
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo card = cards.ElementAt(i);
			for (int j = 0; j < card.DestroyedBySkillList.Count; j++)
			{
				if (card.DestroyedBySkillList.ElementAt(j).Ability == _ability)
				{
					yield return card;
				}
			}
		}
	}
}
