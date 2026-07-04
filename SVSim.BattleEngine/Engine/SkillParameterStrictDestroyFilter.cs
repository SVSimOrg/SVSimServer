using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterStrictDestroyFilter : ISkillCardFilter
{
	private bool isDestroy;

	public SkillParameterStrictDestroyFilter(string value)
	{
		isDestroy = value == "true";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo card) => IsStrictDestroy(card) == isDestroy);
	}

	private bool IsStrictDestroy(IReadOnlyBattleCardInfo card)
	{
		if (card.IsDead && !card.DeathTypeInfo.BanishDestroy && !card.DeathTypeInfo.LeaveByGetOn)
		{
			return card.MetamorphoseCard == null;
		}
		return false;
	}
}
