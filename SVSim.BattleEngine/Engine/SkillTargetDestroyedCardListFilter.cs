using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDestroyedCardListFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoNecromanceZoneCards.Where((IReadOnlyBattleCardInfo pp) => pp.IsDead && !(pp is NullBattleCard))));
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoCemeterys.Where((IReadOnlyBattleCardInfo pp) => pp.IsDead && !(pp is NullBattleCard))));
		return list;
	}
}
