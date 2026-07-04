using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetHealingCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		IEnumerable<IReadOnlyBattleCardInfo> result = new IReadOnlyBattleCardInfo[0];
		if (option.HealingCardAndValue == null)
		{
			return result;
		}
		foreach (IBattlePlayerReadOnlyInfo info in battlePlayerInfos)
		{
			result = from h in option.HealingCardAndValue
				select h.Card into c
				where c.IsPlayer == info.IsPlayer
				select c;
		}
		return result;
	}
}
