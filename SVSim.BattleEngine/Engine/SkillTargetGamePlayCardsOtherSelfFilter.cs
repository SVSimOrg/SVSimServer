using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetGamePlayCardsOtherSelfFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetGamePlayCardsOtherSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(battlePlayerInfo.SkillInfoGamePlayCards);
		}
		if (!_ownerCard.IsInHand || _ownerCard.IsSpell)
		{
			list.Remove(_ownerCard);
		}
		return list;
	}
}
