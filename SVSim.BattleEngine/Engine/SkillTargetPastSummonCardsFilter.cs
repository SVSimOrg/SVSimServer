using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetPastSummonCardsFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	private bool _isTurnEndUpdate;

	public SkillTargetPastSummonCardsFilter(IReadOnlyBattleCardInfo ownerCard, string option)
	{
		_ownerCard = ownerCard;
		_isTurnEndUpdate = option == "turn_end_update";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		BattleManagerBase ins = _ownerCard.SelfBattlePlayer.BattleMgr;
		int turn = ins.CurrentTurn;
		bool isTurnEnd = ins.IsTurnEnd;
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(from c in battlePlayerInfo.SkillInfoGameSummonCards
				where (!_isTurnEndUpdate || !c.IsTurnEnd) && c.Turn >= _ownerCard.DrawTurn && c.IsSelfTurn == _ownerCard.IsPlayer && (!_isTurnEndUpdate || isTurnEnd || !_ownerCard.IsSelfTurn || c.Turn < turn)
				select c.Card);
		}
		return list;
	}
}
