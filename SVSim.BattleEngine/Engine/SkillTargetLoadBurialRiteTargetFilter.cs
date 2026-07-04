using System.Collections.Generic;
using Wizard.Battle;

public class SkillTargetLoadBurialRiteTargetFilter : ISkillTargetFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetLoadBurialRiteTargetFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return _ownerCard.SkillApplyInformation.LoadBurialRiteTargetList();
	}
}
