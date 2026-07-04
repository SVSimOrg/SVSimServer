using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetSelfFilter : ISkillTargetFilter
{
	private readonly IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		yield return _ownerCard;
	}
}
