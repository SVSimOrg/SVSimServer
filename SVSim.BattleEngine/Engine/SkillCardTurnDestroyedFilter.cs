using System.Collections.Generic;
using Wizard.Battle;

public class SkillCardTurnDestroyedFilter : ISkillCardFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillCardTurnDestroyedFilter(IReadOnlyBattleCardInfo card, string option)
	{
		_ownerCard = card;
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return _ownerCard.SelfBattlePlayer.BattleMgr.GetBattlePlayer(_ownerCard.IsPlayer).GetSpecificTurnDestroyCards(_turnPlayerInfo);
	}
}
