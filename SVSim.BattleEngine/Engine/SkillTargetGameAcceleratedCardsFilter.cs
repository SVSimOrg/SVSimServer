using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetGameAcceleratedCardsFilter : ISkillTargetFilter
{
	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IBattlePlayerReadOnlyInfo battlePlayerInfo in battlePlayerInfos)
		{
			foreach (IReadOnlyBattleCardInfo skillInfoGamePlayCard in battlePlayerInfo.SkillInfoGamePlayCards)
			{
				if (skillInfoGamePlayCard.TransformInfo.Type == BattleCardBase.TransformType.Accelerate)
				{
					list.Add(skillInfoGamePlayCard.TransformInfo.OriginalCard);
				}
			}
		}
		return list;
	}
}
