using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetGiveDamageCardFilter : ISkillTargetFilter
{
	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		if (option.FixedDamage.Skill == null)
		{
			return new IReadOnlyBattleCardInfo[0];
		}
		BattleCardBase ownerCard = option.FixedDamage.Skill.SkillPrm.ownerCard;
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			if (ownerCard.IsPlayer == battlePlayerInfo.IsPlayer)
			{
				return new IReadOnlyBattleCardInfo[1] { ownerCard };
			}
		}
		return new IReadOnlyBattleCardInfo[0];
	}
}
