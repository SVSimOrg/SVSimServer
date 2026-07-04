using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetCantAttackAllLastTargetFilter : SkillTargetLastTargetFilter
{
	public SkillTargetCantAttackAllLastTargetFilter(string option)
		: base(option)
	{
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from c in base.Filtering(battlePlayerInfos, option)
			where c.SkillApplyInformation.IsSkillCantAtkAll
			select c;
	}
}
