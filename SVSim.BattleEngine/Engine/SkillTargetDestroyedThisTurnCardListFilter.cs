using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDestroyedThisTurnCardListFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		// Turn is battle-scoped (same for every player in the pair); IsSelfTurn on the mgr's
		// BattlePlayer is equivalent to the mgr-home player's IsSelfTurn. Route through the
		// first battle-player-info's Turn and IsPlayer/IsSelfTurn.
		IBattlePlayerReadOnlyInfo first = battlePlayerInfos.FirstOrDefault();
		if (first == null) return list;
		int turn = first.Turn;
		bool isSelfTurn = (battlePlayerInfos.FirstOrDefault(p => p.IsPlayer) ?? first).IsSelfTurn;
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoNecromanceZoneCards.Where((IReadOnlyBattleCardInfo pp) => pp.IsDead && !(pp is NullBattleCard) && pp.DestroyedTurn == turn && pp.IsDestroySelfTurn == isSelfTurn)));
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoCemeterys.Where((IReadOnlyBattleCardInfo pp) => pp.IsDead && !(pp is NullBattleCard) && pp.DestroyedTurn == turn && pp.IsDestroySelfTurn == isSelfTurn)));
		return list;
	}
}
