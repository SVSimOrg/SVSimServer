using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetGameSummonCardsOtherFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetGameSummonCardsOtherFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(battlePlayerInfo.SkillInfoGameSummonCards.Select((BattlePlayerBase.TurnAndCard c) => c.Card));
		}
		if (option.PlayedCard == _ownerCard)
		{
			list.Remove(_ownerCard);
		}
		return list;
	}
}
