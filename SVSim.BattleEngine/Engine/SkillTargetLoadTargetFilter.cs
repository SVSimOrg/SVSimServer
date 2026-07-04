using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetLoadTargetFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetLoadTargetFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return _ownerCard.SkillApplyInformation.LoadTargetList();
	}
}
