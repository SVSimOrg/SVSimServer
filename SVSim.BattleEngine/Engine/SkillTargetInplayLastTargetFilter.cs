using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetInplayLastTargetFilter : SkillTargetLastTargetFilter
{

	public SkillTargetInplayLastTargetFilter(string option)
		: base(option)
	{
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards).Intersect(base.Filtering(battlePlayerInfos, option));
	}
}
