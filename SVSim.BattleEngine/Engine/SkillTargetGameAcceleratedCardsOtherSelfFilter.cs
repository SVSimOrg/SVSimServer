using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetGameAcceleratedCardsOtherSelfFilter : SkillTargetGameAcceleratedCardsFilter
{
	private IReadOnlyBattleCardInfo _ownerCard;

	public SkillTargetGameAcceleratedCardsOtherSelfFilter(IReadOnlyBattleCardInfo ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		return from c in base.Filtering(battlePlayerInfos, option)
			where c != _ownerCard.TransformInfo.OriginalCard
			select c;
	}
}
