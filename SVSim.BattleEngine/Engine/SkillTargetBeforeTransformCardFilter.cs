using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetBeforeTransformCardFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetBeforeTransformCardFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		BattleCardBase.TransformInformation transformInfo = _ownerCard.TransformInfo;
		if (transformInfo.Type != BattleCardBase.TransformType.Metamorphose && transformInfo.OriginalCard != null)
		{
			list.Add(transformInfo.OriginalCard);
		}
		return list;
	}
}
