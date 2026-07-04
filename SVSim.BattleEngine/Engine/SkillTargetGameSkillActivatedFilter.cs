using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetGameSkillActivatedFilter : ISkillTargetFilter
{
	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < battlePlayerInfos.Count(); i++)
		{
			list.AddRange(battlePlayerInfos.ElementAt(i).SkillInfoGameSkillActivated);
		}
		return list;
	}
}
