using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetDamagedCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from p in battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards)
			where p.MaxLife > p.Life
			select p;
	}
}
