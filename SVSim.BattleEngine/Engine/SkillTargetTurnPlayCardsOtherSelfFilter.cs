using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetTurnPlayCardsOtherSelfFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillTargetTurnPlayCardsOtherSelfFilter(IReadOnlyBattleCardInfo ownerCard, string option)
	{
		_ownerCard = ownerCard;
		_turnPlayerInfo = ((option == "") ? null : new TurnPlayerInfo(option));
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (_turnPlayerInfo == null)
			{
				list.AddRange(battlePlayerInfo.SkillInfoTurnPlayCards);
				continue;
			}
			int turn = battlePlayerInfo.Turn;
			turn -= _turnPlayerInfo.TurnOffset;
			list.AddRange(from c in battlePlayerInfo.SkillInfoGameTurnPlayCards
				where c.Turn == turn
				select c.Card);
		}
		if ((!_ownerCard.IsInHand || _ownerCard.IsSpell) && (_turnPlayerInfo == null || _turnPlayerInfo.TurnOffset == 0))
		{
			list.Remove(_ownerCard);
		}
		return list;
	}
}
