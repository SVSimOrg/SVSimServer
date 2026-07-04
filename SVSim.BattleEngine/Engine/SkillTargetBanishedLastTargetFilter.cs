using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetBanishedLastTargetFilter : SkillTargetLastTargetFilter
{
	public SkillTargetBanishedLastTargetFilter(string option)
		: base(option)
	{
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		list.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoBanishCards.Where((IReadOnlyBattleCardInfo pp) => pp is SpellBattleCard || (pp.IsDead && !(pp is NullBattleCard)))));
		return list.Intersect(base.Filtering(battlePlayerInfos, option));
	}
}
