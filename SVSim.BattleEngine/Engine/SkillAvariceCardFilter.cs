using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAvariceCardFilter : ISkillCardFilter
{
	public readonly bool _isAvarice;

	public SkillAvariceCardFilter(bool isAvarice)
	{
		_isAvarice = isAvarice;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		for (int i = 0; i < cards.Count(); i++)
		{
			BattleCardBase battleCardBase = cards.ElementAt(i) as BattleCardBase;
			int turnDrawCount = battleCardBase.SelfBattlePlayer.SkillInfoTurnDrawCards.Count();
			if ((battleCardBase.SelfBattlePlayer.SkillInfoClass.SkillApplyInformation.IsForceAvarice || SkillConditionAvarice.IsAvarice(turnDrawCount)) == _isAvarice)
			{
				yield return battleCardBase;
			}
		}
	}
}
