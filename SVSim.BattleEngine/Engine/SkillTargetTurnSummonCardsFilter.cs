using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetTurnSummonCardsFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		// Turn is battle-scoped; IsSelfTurn on the mgr's home player is equivalent to iterating
		// for the p.IsPlayer entry. Route through the first battle-player-info.
		IBattlePlayerReadOnlyInfo _f = battlePlayerInfos.FirstOrDefault();
		if (_f == null) return list;
		int turn = _f.Turn;
		bool isSelfTurn = (battlePlayerInfos.FirstOrDefault(p => p.IsPlayer) ?? _f).IsSelfTurn;
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			list.AddRange(from c in battlePlayerInfo.SkillInfoGameSummonCards
				where c.Turn == turn && c.IsSelfTurn == isSelfTurn
				select c.Card);
		}
		return list;
	}
}
