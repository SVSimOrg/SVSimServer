using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetNecromanceFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.NecromanceCard != null)
		{
			yield return option.NecromanceCard;
		}
	}
}
