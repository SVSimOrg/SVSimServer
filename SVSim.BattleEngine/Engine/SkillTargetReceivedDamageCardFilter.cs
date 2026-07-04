using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetReceivedDamageCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		IEnumerable<IReadOnlyBattleCardInfo> result = new IReadOnlyBattleCardInfo[0];
		if (option.DamageCards == null)
		{
			return result;
		}
		foreach (IBattlePlayerReadOnlyInfo info in battlePlayerInfos)
		{
			result = option.DamageCards.Where((IReadOnlyBattleCardInfo c) => c.IsPlayer == info.IsPlayer);
		}
		return result;
	}
}
