using System.Collections.Generic;
using Wizard.Battle;

public class SkillLoadTargetFilter : ISkillCardFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	public SkillLoadTargetFilter(IReadOnlyBattleCardInfo card)
	{
		_ownerCard = card;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return _ownerCard.SkillApplyInformation.LoadTargetList();
	}
}
