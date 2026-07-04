using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillPlayCardTypeFilter : ISkillCardFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	private TurnPlayerInfo _turnPlayerInfo;

	public SkillPlayCardTypeFilter(IReadOnlyBattleCardInfo ownerCard, string option)
	{
		_ownerCard = ownerCard;
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		IEnumerable<IReadOnlyBattleCardInfo> playCardList = GetPlayCardList();
		bool playUnit = playCardList.Any((IReadOnlyBattleCardInfo c) => c.IsUnit);
		bool playSpell = playCardList.Any((IReadOnlyBattleCardInfo c) => c.IsSpell);
		bool playField = playCardList.Any((IReadOnlyBattleCardInfo c) => c.IsField || c.IsChantField);
		return cards.Where((IReadOnlyBattleCardInfo c) => (playUnit && c.IsUnit) || (playSpell && c.IsSpell) || (playField && (c.IsField || c.IsChantField)));
	}

	public IEnumerable<IReadOnlyBattleCardInfo> GetPlayCardList()
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (_ownerCard != null && _ownerCard is BattleCardBase battleCardBase)
		{
			BattlePlayerBase battlePlayerBase = (_turnPlayerInfo.IsSelfPlayer ? battleCardBase.SelfBattlePlayer : battleCardBase.OpponentBattlePlayer);
			int turn = battlePlayerBase.Turn - _turnPlayerInfo.TurnOffset;
			list.AddRange(from c in battlePlayerBase.SkillInfoGameTurnPlayCards
				where c.Turn == turn
				select c.Card);
		}
		return list;
	}
}
