using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetFusionIngredientedThisTurnCardList : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		IBattlePlayerReadOnlyInfo _f = battlePlayerInfos.FirstOrDefault();
		if (_f == null) return list;
		int turn = _f.Turn;
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoFusionIngredientList.Where((IReadOnlyBattleCardInfo pp) => pp.FusionedTurn == turn)));
		return list;
	}
}
